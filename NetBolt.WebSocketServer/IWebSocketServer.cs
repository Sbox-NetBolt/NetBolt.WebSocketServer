using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Options;

namespace NetBolt.WebSocket;

/// <summary>
/// Defines something that can be a server accepting <see cref="IWebSocketClient"/>s.
/// </summary>
public interface IWebSocketServer
{
	/// <summary>
	/// A read-only version of the <see cref="WebSocketServerOptions"/> that the server was constructed with.
	/// </summary>
	IReadOnlyWebSocketServerOptions Options { get; }

	/// <summary>
	/// A read-only list of all clients that are connected to the server.
	/// </summary>
	IReadOnlyList<IWebSocketClient> Clients { get; }

	/// <summary>
	/// Whether or not the server is running.
	/// </summary>
	bool Running { get; }

	/// <summary>
	/// Starts the server.
	/// </summary>
	void Start();
	/// <summary>
	/// Stops the server.
	/// </summary>
	/// <returns>The async task that spawns from the invoke.</returns>
	Task StopAsync();

	/// <summary>
	/// Accepts a client to the server.
	/// </summary>
	/// <param name="client">The client to accept.</param>
	void AcceptClient( IWebSocketClient client );

	/// <summary>
	/// Disconnects a client from the server.
	/// </summary>
	/// <param name="client">The client to disconnect/</param>
	/// <param name="reason">The reason for the disconnect</param>
	/// <param name="strReason">The string representation of the reason for the disconnect.</param>
	/// <returns>The async task that spawns from the invoke.</returns>
	Task DisconnectClientAsync( IWebSocketClient client,
		WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "" );
	/// <summary>
	/// Pings a client in the server.
	/// </summary>
	/// <param name="client">The client to ping.</param>
	/// <param name="timeout">The time in seconds before the ping is timed out.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the amount of time taken in milliseconds for the round trip.</returns>
	ValueTask<int> PingClientAsync( IWebSocketClient client, int timeout = int.MaxValue );

	/// <summary>
	/// Invoked when a client has connected to the server.
	/// <remarks>This does not mean they are capable of receiving messages yet. See <see cref="OnClientUpgraded"/>.</remarks>
	/// </summary>
	/// <param name="client">The client that has connected.</param>
	void OnClientConnected( IWebSocketClient client );
	/// <summary>
	/// Invoked when a client has been upgraded to the web socket protocol.
	/// </summary>
	/// <param name="client">The client that has been upgraded.</param>
	void OnClientUpgraded( IWebSocketClient client );
	/// <summary>
	/// Invoked when a client has disconnected from the server.
	/// </summary>
	/// <param name="client">The client that has disconnected.</param>
	/// <param name="reason">The reason for the client disconnecting.</param>
	/// <param name="error">The error associated with the disconnect.</param>
	void OnClientDisconnected( IWebSocketClient client, WebSocketDisconnectReason reason, WebSocketError? error );

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Binary"/> message to clients.
	/// </summary>
	/// <param name="to">The clients to send the message to.</param>
	/// <param name="bytes">The binary data to send.</param>
	void QueueSend( To to, byte[] bytes );
	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Text"/> message to clients.
	/// </summary>
	/// <param name="to">The clients to send the message to.</param>
	/// <param name="message">The message to send.</param>
	void QueueSend( To to, string message );
}
