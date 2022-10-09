using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Exceptions;
using NetBolt.WebSocket.Extensions;
using NetBolt.WebSocket.Options;
using NetBolt.WebSocket.Utility;

namespace NetBolt.WebSocket;

/// <summary>
/// A basic implementation of a web socket client wrapper around a <see cref="TcpClient"/>.
/// </summary>
public class WebSocketClient : IWebSocketClient, IDisposable, IAsyncDisposable
{
	/// <summary>
	/// Whether or not this client is connected to the server.
	/// <remarks>This does not mean they are capable of receiving messages yet. See <see cref="ConnectedAndUpgraded"/>.</remarks>
	/// </summary>
	public bool Connected { get; private set; }
	/// <summary>
	/// Whether or not this client is connected to the server and has been upgraded to the web socket protocol.
	/// </summary>
	public bool ConnectedAndUpgraded => Connected && _upgraded;

	/// <summary>
	/// The Internet Protocol (IP) address of the remote client.
	/// </summary>
	public IPAddress IpAddress
	{
		get => _socket.Client?.RemoteEndPoint is IPEndPoint ipEndPoint ? ipEndPoint.Address : IPAddress.None;
	}
	/// <summary>
	/// The port number of the remote clients socket.
	/// </summary>
	public int Port
	{
		get => _socket.Client?.RemoteEndPoint is IPEndPoint ipEndPoint ? ipEndPoint.Port : -1;
	}

	/// <summary>
	/// The clients current ping time in milliseconds.
	/// <remarks>This will only provide valid results if <see cref="WebSocketServerOptions.AutoPing"/> is enabled.</remarks>
	/// </summary>
	public int Ping { get; private set; }

	/// <summary>
	/// The server that this web socket is linked to.
	/// </summary>
	private readonly IWebSocketServer _server;
	/// <summary>
	/// The underlying socket this client controls.
	/// </summary>
	private readonly TcpClient _socket;
	/// <summary>
	/// The queue of messages to be sent to the client.
	/// </summary>
	private readonly ConcurrentQueue<(WebSocketOpCode, byte[])> _outgoingQueue = new();

	/// <summary>
	/// The asynchronous reading task this client is running.
	/// </summary>
	private Task<WebSocketError?> _readTask = Task.FromResult<WebSocketError?>( null );
	/// <summary>
	/// The asynchronous writing task this client is running.
	/// </summary>
	private Task _writeTask = Task.CompletedTask;
	/// <summary>
	/// The asynchronous ping task this client is running.
	/// </summary>
	private Task _pingTask = Task.CompletedTask;
	/// <summary>
	/// Whether or not this client has been upgraded to the web socket protocol.
	/// </summary>
	private bool _upgraded;
	/// <summary>
	/// Whether or not this client has replied with a pong message.
	/// </summary>
	private bool _ponged;
	/// <summary>
	/// Whether or not this client is in the process of disconnecting.
	/// </summary>
	private bool _disconnecting;

	/// <summary>
	/// Initializes a new instance of <see cref="WebSocketClient"/> with its underlying socket and the server it is a part of.
	/// </summary>
	/// <param name="socket">The underlying socket of the client.</param>
	/// <param name="server">The server that this client is a part of.</param>
	public WebSocketClient( TcpClient socket, IWebSocketServer server )
	{
		_socket = socket;
		_server = server;
	}

	/// <summary>
	/// Disconnects the client from the server.
	/// </summary>
	/// <param name="reason">The reason for the disconnect.</param>
	/// <param name="strReason">The string representation of the reason for the disconnect.</param>
	/// <param name="error">The error associated with the disconnect if applicable.</param>
	/// <returns>The async task that spawns from the invoke.</returns>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	public async Task DisconnectAsync( WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested,
		string strReason = "", WebSocketError? error = null )
	{
		this.ThrowIfDisconnected();

		_disconnecting = true;
		await _writeTask.ConfigureAwait( false );
		await Send( WebSocketOpCode.Close, WebSocketFrame.FormatCloseData( reason, strReason ) ).ConfigureAwait( false );
		Disconnect( reason, error );
	}

	/// <summary>
	/// Acts as the main loop for the client to handle its read/write logic.
	/// <remarks>The client should be disconnected once this is completed.</remarks>
	/// </summary>
	/// <returns>The async task that spawns from the invoke.</returns>
	/// <exception cref="ClientConnectedException">Thrown when using this method while the client is connected.</exception>
	public async Task HandleAsync()
	{
		this.ThrowIfConnected();

		Connected = true;
		OnConnected();
		WebSocketDisconnectReason? disconnectReason = null;
		WebSocketError? errorType = null;

		_readTask = HandleReadAsync();
		_writeTask = HandleWriteAsync();
		if ( _server.Options.ReadOnlyAutoPing.Enabled )
			_pingTask = HandlePingAsync();

		await Task.WhenAll( _readTask, _writeTask, _pingTask ).ConfigureAwait( false );
		if ( !Connected )
			return;

		disconnectReason = _readTask.Result is null ? disconnectReason : WebSocketDisconnectReason.Error;
		await DisconnectAsync( disconnectReason ?? WebSocketDisconnectReason.None, string.Empty, errorType ).ConfigureAwait( false );
	}

	/// <summary>
	/// Pings the client.
	/// </summary>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the amount of time taken in milliseconds for the round trip.</returns>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected. -1 will be returned if it timed out.</exception>
	public async ValueTask<int> PingAsync( int timeout = int.MaxValue )
	{
		this.ThrowIfDisconnected();

		QueueSend( WebSocketOpCode.Ping, Array.Empty<byte>() );

		var sw = Stopwatch.StartNew();
		while ( !_ponged )
		{
			if ( !Connected || _disconnecting )
				break;

			await Task.Delay( 1 ).ConfigureAwait( false );
		}
		sw.Stop();

		_ponged = false;
		return (int)Math.Floor( sw.Elapsed.TotalMilliseconds );
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Binary"/> message to the client.
	/// </summary>
	/// <param name="bytes">The binary data to send.</param>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	public void QueueSend( byte[] bytes )
	{
		this.ThrowIfDisconnected();

		QueueSend( WebSocketOpCode.Binary, bytes );
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Text"/> message to the client.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	public void QueueSend( string message )
	{
		this.ThrowIfDisconnected();

		QueueSend( WebSocketOpCode.Text, Encoding.UTF8.GetBytes( message ) );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="WebSocketClient"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="WebSocketClient"/>.</returns>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append( nameof( WebSocketClient ) );

		if ( _socket.Client is not null )
		{
			sb.Append( '(' );
			sb.Append( IpAddress );
			sb.Append( ':' );
			sb.Append( Port );
			sb.Append( ')' );
		}
		else
			sb.Append( "(Disconnected)" );

		return sb.ToString();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose()
	{
		GC.SuppressFinalize( this );
		DisconnectAsync().Wait();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public virtual async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize( this );
		await DisconnectAsync();
	}

	/// <summary>
	/// Invoked when the client is being verified for their handshake.
	/// </summary>
	/// <param name="headers">The handshake headers sent by the client.</param>
	/// <param name="request">The full request the client sent.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the whether or not to accept the clients handshake.</returns>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected async virtual ValueTask<bool> VerifyHandshake( IReadOnlyDictionary<string, string> headers, string request )
	{
		this.ThrowIfDisconnected();

		return true;
	}

	/// <summary>
	/// Invoked when the client has connected to the server.
	/// <remarks>This does not mean the client is capable of receiving messages yet. See <see cref="OnUpgraded"/>.</remarks>
	/// </summary>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected virtual void OnConnected()
	{
		this.ThrowIfDisconnected();

		_server.OnClientConnected( this );
	}

	/// <summary>
	/// Invoked when the client has been upgraded to the web socket protocol.
	/// </summary>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected virtual void OnUpgraded()
	{
		this.ThrowIfDisconnected();

		_server.OnClientUpgraded( this );
	}

	/// <summary>
	/// Invoked when the client has disconnected from the server.
	/// </summary>
	/// <param name="reason">The reason for disconnecting.</param>
	/// <param name="error">The error associated with the disconnect.</param>
	protected virtual void OnDisconnected( WebSocketDisconnectReason reason, WebSocketError? error )
	{
		this.ThrowIfConnected();

		_server.OnClientDisconnected( this, reason, error );
	}

	/// <summary>
	/// Invoked when a <see cref="WebSocketOpCode.Binary"/> message has been received.
	/// </summary>
	/// <param name="bytes">The data that was sent by the client.</param>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected virtual void OnData( ReadOnlySpan<byte> bytes )
	{
		this.ThrowIfDisconnected();
	}

	/// <summary>
	/// Invoked when a <see cref="WebSocketOpCode.Text"/> message has been received.
	/// </summary>
	/// <param name="message">The message that was sent by the client.</param>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected virtual void OnMessage( string message )
	{
		this.ThrowIfDisconnected();
	}

	/// <summary>
	/// Handles reading a message from the client socket.
	/// </summary>
	/// <returns>The error that was experienced if applicable.</returns>
	private async Task<WebSocketError?> HandleReadAsync()
	{
		while ( Connected && !_disconnecting )
		{
			NetworkStream stream;
			do
			{
				if ( !_socket.TryGetStream( out var innerStream ) )
					return WebSocketError.StreamDisposed;
				stream = innerStream;

				while ( !stream.DataAvailable && Connected )
				{
					await Task.Delay( 1 ).ConfigureAwait( false );

					if ( !_socket.TryGetStream( out innerStream ) )
						return WebSocketError.StreamDisposed;
					stream = innerStream;
				}
			} while ( !stream.DataAvailable && Connected );

			if ( !Connected )
				return null;

			if ( _socket.Available > _server.Options.ReadOnlyMessaging.MaxMessageReceiveBytes )
				return WebSocketError.MessageTooLarge;

			var bytes = new byte[_socket.Available];
			_ = await stream.ReadAsync( bytes ).ConfigureAwait( false );
			if ( !Connected )
				return null;

			if ( !_upgraded )
			{
				var success = await HandleHandshake( bytes ).ConfigureAwait( false );
				if ( !success )
					return !Connected ? null : WebSocketError.UpgradeFail;

				_upgraded = true;
				OnUpgraded();
				continue;
			}

			var frame = WebSocketFrame.ParseClientFrame( bytes, _server.Options.ReadOnlyMessaging.MaxStackAllocDecodeBytes );
			if ( !frame.Finished )
				return WebSocketError.MessageUnfinished;

			if ( !frame.Masked )
				return WebSocketError.MissingMask;

			HandleFrame( frame );
			await Task.Delay( 1 ).ConfigureAwait( false );
		}

		return null;
	}

	/// <summary>
	/// Handles writing messages to the client socket.
	/// </summary>
	private async Task HandleWriteAsync()
	{
		while ( Connected && !_disconnecting )
		{
			while ( _upgraded && _outgoingQueue.TryDequeue( out var data ) )
			{
				await Send( data.Item1, data.Item2 ).ConfigureAwait( false );
				if ( data.Item1 != WebSocketOpCode.Close )
					continue;

				_ = DisconnectAsync();
				return;
			}

			await Task.Delay( 1 ).ConfigureAwait( false );
		}
	}

	/// <summary>
	/// Handles pinging the client on an interval.
	/// </summary>
	private async Task HandlePingAsync()
	{
		while ( Connected && !_disconnecting )
		{
			var sw = Stopwatch.StartNew();
			while ( sw.Elapsed.TotalSeconds < _server.Options.ReadOnlyAutoPing.Interval * 1000 )
			{
				await Task.Delay( 1000 ).ConfigureAwait( false );
				if ( !ConnectedAndUpgraded || _disconnecting )
					return;
			}

			var ping = await PingAsync( _server.Options.ReadOnlyAutoPing.Timeout );
			if ( ping == -1 )
				_ = DisconnectAsync( WebSocketDisconnectReason.Timeout );
			else
				Ping = ping;
		}
	}

	/// <summary>
	/// Handles the initial handshake to the client.
	/// </summary>
	/// <param name="bytes">The handshake message received.</param>
	/// <returns>Whether or not the handshake succeeded.</returns>
	private async ValueTask<bool> HandleHandshake( byte[] bytes )
	{
		if ( !_socket.TryGetStream( out var stream ) )
			return false;

		var request = Encoding.UTF8.GetString( bytes );
		if ( !request.StartsWith( "GET" ) )
			return false;

		var headers = WebSocketHandshake.GetRequestHeaders( request );
		if ( !await VerifyHandshake( headers, request ) )
			return false;

		var response = WebSocketHandshake.GetUpgradeResponse( headers["Sec-WebSocket-Key"] );
		await stream.WriteAsync( response ).ConfigureAwait( false );
		return Connected;
	}

	/// <summary>
	/// Handles an incoming frame.
	/// </summary>
	/// <param name="frame">The frame that was received from the client.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the frames <see cref="WebSocketOpCode"/> is invalid.</exception>
	private void HandleFrame( WebSocketFrame frame )
	{
		try
		{
			switch ( frame.OpCode )
			{
				case WebSocketOpCode.Continuation:
					break;
				case WebSocketOpCode.Text:
					var message = Encoding.UTF8.GetString( frame.Data.Span );
					if ( message == _server.Options.DisconnectPhrase )
					{
						_ = DisconnectAsync().ConfigureAwait( false );
						break;
					}

					OnMessage( message );
					break;
				case WebSocketOpCode.Binary:
					OnData( frame.Data.Span );
					break;
				case WebSocketOpCode.NonControl0:
					break;
				case WebSocketOpCode.NonControl1:
					break;
				case WebSocketOpCode.NonControl2:
					break;
				case WebSocketOpCode.NonControl3:
					break;
				case WebSocketOpCode.NonControl4:
					break;
				case WebSocketOpCode.Close:
					_ = DisconnectAsync().ConfigureAwait( false );
					break;
				case WebSocketOpCode.Ping:
					break;
				case WebSocketOpCode.Pong:
					break;
				case WebSocketOpCode.Control0:
					break;
				case WebSocketOpCode.Control1:
					break;
				case WebSocketOpCode.Control2:
					break;
				case WebSocketOpCode.Control3:
					break;
				case WebSocketOpCode.Control4:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( frame.OpCode ), frame.OpCode, null );
			}
		}
		catch ( Exception )
		{
			_ = DisconnectAsync( WebSocketDisconnectReason.Error, "Exception occurred during handling", WebSocketError.HandlingException ).ConfigureAwait( false );
			throw;
		}
	}

	/// <summary>
	/// Disconnects the client and cleans up.
	/// </summary>
	/// <param name="reason">The reason for disconnecting.</param>
	/// <param name="error">The error associated with the disconnect.</param>
	private void Disconnect( WebSocketDisconnectReason reason, WebSocketError? error = null )
	{
		if ( !Connected )
			return;

		Connected = false;
		OnDisconnected( reason, error );
		_socket.Close();
		_outgoingQueue.Clear();
		_upgraded = false;
		_ponged = false;
		_disconnecting = false;
	}

	/// <summary>
	/// Queues a message for being sent to the client.
	/// </summary>
	/// <param name="opCode">The <see cref="WebSocketOpCode"/> of the message.</param>
	/// <param name="data">The data payload of the message.</param>
	private void QueueSend( WebSocketOpCode opCode, byte[] data )
	{
		_outgoingQueue.Enqueue( (opCode, data) );
	}

	/// <summary>
	/// sends a message to the client.
	/// </summary>
	/// <param name="opCode">The <see cref="WebSocketOpCode"/> of the message.</param>
	/// <param name="data">The data payload of the message.</param>
	private async Task Send( WebSocketOpCode opCode, ReadOnlyMemory<byte> data )
	{
		if ( !_socket.TryGetStream( out var stream ) )
			return;

		try
		{
			if ( WebSocketFrame.GetTotalMessageSize( data.Span, _server.Options.ReadOnlyMessaging.MaxFrameSendBytes ) >
				 _server.Options.ReadOnlyMessaging.MaxMessageSendBytes )
				throw new OverflowException();

			foreach ( var frame in WebSocketFrame.FrameMulti( opCode, data, _server.Options.ReadOnlyMessaging.MaxFrameSendBytes ) )
			{
				if ( frame.GetByteSize() > _server.Options.ReadOnlyMessaging.MaxFrameSendBytes )
					throw new OverflowException();

				await stream.WriteAsync( frame.GetBytes( _server.Options.ReadOnlyMessaging.MaxStackAllocFrameBytes ) ).ConfigureAwait( false );
			}
		}
		catch ( IOException )
		{
			if ( !_disconnecting )
				_ = DisconnectAsync( WebSocketDisconnectReason.Error, "Failed to send message", WebSocketError.WriteError );
		}
		catch ( OverflowException )
		{
			if ( !_disconnecting )
				_ = DisconnectAsync( WebSocketDisconnectReason.Error, "Tried to send a message that was too large", WebSocketError.MessageTooLarge );
		}
	}
}
