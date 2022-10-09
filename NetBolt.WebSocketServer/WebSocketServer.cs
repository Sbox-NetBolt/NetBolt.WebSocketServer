using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Exceptions;
using NetBolt.WebSocket.Options;
using NetBolt.WebSocket.Utility;

namespace NetBolt.WebSocket;

/// <summary>
/// A basic implementation of a web socket server wrapper around a <see cref="TcpListener"/>.
/// </summary>
public class WebSocketServer : IWebSocketServer, IDisposable, IAsyncDisposable
{
	/// <summary>
	/// The options that this server was created with.
	/// </summary>
	public IReadOnlyWebSocketServerOptions Options { get; }

	/// <summary>
	/// A read-only list of all clients that are connected to the server.
	/// </summary>
	public IReadOnlyList<IWebSocketClient> Clients => ClientSockets;
	/// <summary>
	/// A read-only list of all clients that are connected to the server and have been upgraded to the web socket protocol.
	/// </summary>
	public IReadOnlyList<IWebSocketClient> UpgradedClients => ClientSockets.Where( client => client.ConnectedAndUpgraded ).ToList();

	/// <summary>
	/// A list of all clients that are connected to the server.
	/// </summary>
	private List<IWebSocketClient> ClientSockets { get; } = new();

	/// <summary>
	/// Whether or not the server is running.
	/// </summary>
	public bool Running { get; private set; }
	/// <summary>
	/// Whether or not a stop to the server has been requested.
	/// </summary>
	protected bool StopRequested { get; private set; }

	/// <summary>
	/// The underlying server accepting connections.
	/// </summary>
	private readonly TcpListener _server;
	/// <summary>
	/// The asynchronous task to accept clients to the server.
	/// </summary>
	private Task? _acceptClientsTask;
	/// <summary>
	/// A dictionary containing a map of clients to their handling task.
	/// </summary>
	private readonly Dictionary<IWebSocketClient, Task> _clientTasks = new();

	/// <summary>
	/// Initializes a new instance of <see cref="WebSocketServer"/> with the provided configuration.
	/// </summary>
	/// <param name="options">The configuration for the server.</param>
	public WebSocketServer( IReadOnlyWebSocketServerOptions options )
	{
		Options = options;
		_server = new TcpListener( options.IpAddress, options.Port );
	}

	/// <summary>
	/// Starts the server.
	/// </summary>
	/// <exception cref="ServerRunningException">Thrown when using this method while the server is running.</exception>
	public void Start()
	{
		this.ThrowIfRunning();

		_server.Start();
		_acceptClientsTask = AcceptClientsAsync();
		Running = true;
	}

	/// <summary>
	/// Stops the server.
	/// </summary>
	/// <returns>The async task that spawns from the invoke.</returns>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	public async Task StopAsync()
	{
		this.ThrowIfNotRunning();

		StopRequested = true;
		var tasks = new List<Task>();
		if ( _acceptClientsTask is not null )
			tasks.Add( _acceptClientsTask );

		tasks.AddRange( _clientTasks.Values );
		var clients = ClientSockets.ToImmutableArray();
		foreach ( var client in clients )
			tasks.Add( DisconnectClientAsync( client, WebSocketDisconnectReason.ServerShutdown, "Server is shutting down" ) );

		await Task.WhenAll( tasks ).ConfigureAwait( false );

		_clientTasks.Clear();
		_server.Stop();
		Running = false;
		StopRequested = false;
	}

	/// <summary>
	/// Accepts a client to the server.
	/// </summary>
	/// <param name="client">The client to accept.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	public virtual void AcceptClient( IWebSocketClient client )
	{
		this.ThrowIfNotRunning();

		ClientSockets.Add( client );
		_clientTasks.Add( client, client.HandleAsync() );
	}

	/// <summary>
	/// Disconnects a client from the server.
	/// </summary>
	/// <param name="client">The client to disconnect/</param>
	/// <param name="reason">The reason for the disconnect</param>
	/// <param name="strReason">The string representation of the reason for the disconnect.</param>
	/// <returns>The async task that spawns from the invoke.</returns>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	/// <exception cref="ClientMissingException">Thrown when disconnecting a client that does not belong to this server.</exception>
	public async Task DisconnectClientAsync( IWebSocketClient client,
		WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "" )
	{
		this.ThrowIfNotRunning();

		if ( !ClientSockets.Contains( client ) )
			throw new ClientMissingException( this, client );

		await client.DisconnectAsync( reason, strReason ).ConfigureAwait( false );
	}

	/// <summary>
	/// Pings a client in the server.
	/// </summary>
	/// <param name="client">The client to ping.</param>
	/// <param name="timeout">The time in seconds before the ping is timed out.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the amount of time taken in milliseconds for the round trip.</returns>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	/// <exception cref="ClientMissingException">Thrown when pinging a client that does not belong to this server.</exception>
	public async ValueTask<int> PingClientAsync( IWebSocketClient client, int timeout = int.MaxValue )
	{
		this.ThrowIfNotRunning();

		if ( !ClientSockets.Contains( client ) )
			throw new ClientMissingException( this, client );

		return await client.PingAsync();
	}

	/// <summary>
	/// Invoked when a client has connected to the server.
	/// <remarks>This does not mean they are capable of receiving messages yet. See <see cref="OnClientUpgraded"/>.</remarks>
	/// </summary>
	/// <param name="client">The client that has connected.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	/// <exception cref="ClientMissingException">Thrown when pinging a client that does not belong to this server.</exception>
	public virtual void OnClientConnected( IWebSocketClient client )
	{
		this.ThrowIfNotRunning();

		if ( !ClientSockets.Contains( client ) )
			throw new ClientMissingException( this, client );
	}

	/// <summary>
	/// Invoked when a client has been upgraded to the web socket protocol.
	/// </summary>
	/// <param name="client">The client that has been upgraded.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	/// <exception cref="ClientMissingException">Thrown when pinging a client that does not belong to this server.</exception>
	public virtual void OnClientUpgraded( IWebSocketClient client )
	{
		this.ThrowIfNotRunning();

		if ( !ClientSockets.Contains( client ) )
			throw new ClientMissingException( this, client );
	}

	/// <summary>
	/// Invoked when a client has disconnected from the server.
	/// </summary>
	/// <param name="client">The client that has disconnected.</param>
	/// <param name="reason">The reason for the client disconnecting.</param>
	/// <param name="error">The error associated with the disconnect.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	/// <exception cref="ClientMissingException">Thrown when pinging a client that does not belong to this server.</exception>
	public virtual void OnClientDisconnected( IWebSocketClient client, WebSocketDisconnectReason reason, WebSocketError? error )
	{
		this.ThrowIfNotRunning();

		if ( !ClientSockets.Contains( client ) )
			throw new ClientMissingException( this, client );

		ClientSockets.Remove( client );
		_clientTasks.Remove( client );
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Binary"/> message to clients.
	/// </summary>
	/// <param name="to">The clients to send the message to.</param>
	/// <param name="bytes">The binary data to send.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	public void QueueSend( To to, byte[] bytes )
	{
		this.ThrowIfNotRunning();

		foreach ( var client in to )
			client.QueueSend( bytes );
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Text"/> message to clients.
	/// </summary>
	/// <param name="to">The clients to send the message to.</param>
	/// <param name="message">The message to send.</param>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	public void QueueSend( To to, string message )
	{
		this.ThrowIfNotRunning();

		foreach ( var client in to )
			client.QueueSend( message );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="WebSocketServer"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="WebSocketServer"/>.</returns>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append( nameof( WebSocketServer ) );

		if ( Running )
		{
			sb.Append( "(IP: " );
			sb.Append( Options.IpAddress );
			sb.Append( ", Port: " );
			sb.Append( Options.Port );
			sb.Append( ", Clients Connected: " );
			sb.Append( ClientSockets.Count );
			sb.Append( ')' );
		}
		else
			sb.Append( "(Not running)" );

		return sb.ToString();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose()
	{
		GC.SuppressFinalize( this );
		StopAsync().Wait();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public virtual async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize( this );
		await StopAsync();
	}

	/// <summary>
	/// Creates a socket client for the provided <see cref="TcpClient"/>.
	/// </summary>
	/// <param name="client">The client socket to create a wrapper around.</param>
	/// <returns>The created socket client.</returns>
	/// <exception cref="ServerNotRunningException">Thrown when using this method while the server is not running.</exception>
	protected virtual IWebSocketClient CreateClient( TcpClient client )
	{
		this.ThrowIfNotRunning();

		return new WebSocketClient( client, this );
	}

	/// <summary>
	/// The asynchronous handler for accepting clients to the server.
	/// </summary>
	private async Task AcceptClientsAsync()
	{
		while ( !StopRequested )
		{
			try
			{
				var tokenSource = new CancellationTokenSource();
				tokenSource.CancelAfter( TimeSpan.FromMilliseconds( 500 ) );
				var client = await _server.AcceptTcpClientAsync( tokenSource.Token ).ConfigureAwait( false );
				if ( StopRequested )
					return;

				var socketClient = CreateClient( client );
				AcceptClient( socketClient );
			}
			catch ( OperationCanceledException )
			{
			}
		}
	}
}
