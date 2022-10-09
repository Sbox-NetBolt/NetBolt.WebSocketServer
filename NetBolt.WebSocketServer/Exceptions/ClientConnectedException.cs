namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// Thrown when using a <see cref="IWebSocketClient"/> that is connected to a <see cref="IWebSocketServer"/> when it should not be.
/// </summary>
public sealed class ClientConnectedException : WebSocketException
{
	/// <summary>
	/// The <see cref="IWebSocketClient"/> that is connected.
	/// </summary>
	public IWebSocketClient Client { get; }

	public ClientConnectedException( IWebSocketClient client ) : base( "Attempted to use a connected client when it should not be" )
	{
		Client = client;
	}
}
