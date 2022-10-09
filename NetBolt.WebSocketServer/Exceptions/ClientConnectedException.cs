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

	/// <summary>
	/// Initializes a new instance of the <see cref="ClientConnectedException"/> class with the client that caused the exception.
	/// </summary>
	/// <param name="client">The client that is connected.</param>
	public ClientConnectedException( IWebSocketClient client ) : base( "Attempted to use a connected client when it should not be" )
	{
		Client = client;
	}
}
