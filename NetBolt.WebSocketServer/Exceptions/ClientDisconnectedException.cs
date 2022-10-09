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
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ClientDisconnectedException"/> class with the client that caused the exception.
	/// </summary>
	/// <param name="client">The client that is disconnected.</param>
	public ClientDisconnectedException( IWebSocketClient client ) : base( "Attempted to use a disconnected client" )
	{
		Client = client;
	}
}
