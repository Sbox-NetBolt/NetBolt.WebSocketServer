using System.Threading.Tasks;
using NetBolt.WebSocket.Enums;

namespace NetBolt.WebSocket;

/// <summary>
/// Defines something that can be a client to a <see cref="IWebSocketServer"/>.
/// </summary>
public interface IWebSocketClient
{
	/// <summary>
	/// Whether or not this client is connected to the server.
	/// <remarks>This does not mean they are capable of receiving messages yet. See <see cref="ConnectedAndUpgraded"/>.</remarks>
	/// </summary>
	bool Connected { get; }
	/// <summary>
	/// Whether or not this client is connected to the server and has been upgraded to the web socket protocol.
	/// </summary>
	bool ConnectedAndUpgraded { get; }

	/// <summary>
	/// The clients current ping time in milliseconds.
	/// </summary>
	public int Ping { get; }

	/// <summary>
	/// Disconnects the client from the server.
	/// </summary>
	/// <param name="reason">The reason for the disconnect.</param>
	/// <param name="strReason">The string representation of the reason for the disconnect.</param>
	/// <param name="error">The error associated with the disconnect if applicable.</param>
	/// <returns>The async task that spawns from the invoke.</returns>
	Task DisconnectAsync( WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "",
		WebSocketError? error = null );
	/// <summary>
	/// Acts as the main loop for the client to handle its read/write logic.
	/// <remarks>The client should be disconnected once this is completed.</remarks>
	/// </summary>
	/// <returns>The async task that spawns from the invoke.</returns>
	Task HandleAsync();
	/// <summary>
	/// Pings the client.
	/// </summary>
	/// <param name="timeout">The time in seconds before the ping is timed out.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the amount of time taken in milliseconds for the round trip. -1 will be returned if it timed out.</returns>
	ValueTask<int> PingAsync( int timeout = int.MaxValue );

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Binary"/> message to the client.
	/// </summary>
	/// <param name="bytes">The binary data to send.</param>
	void QueueSend( byte[] bytes );
	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Text"/> message to the client.
	/// </summary>
	/// <param name="message">The message to send.</param>
	void QueueSend( string message );
}
