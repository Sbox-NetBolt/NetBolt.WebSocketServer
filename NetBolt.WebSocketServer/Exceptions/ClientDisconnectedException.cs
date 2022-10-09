namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// Thrown when using a <see cref="IWebSocketClient"/> that is not connected to a <see cref="IWebSocketServer"/>.
/// </summary>
public sealed class ClientDisconnectedException : WebSocketException
{
	/// <summary>
	/// The <see cref="IWebSocketClient"/> that is disconnected.
	/// </summary>
	public IWebSocketClient Client { get; }

	public ClientDisconnectedException( IWebSocketClient client ) : base( "Attempted to use a disconnected client" )
	{
		Client = client;
	}
}
