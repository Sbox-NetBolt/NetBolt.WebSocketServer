namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// Thrown when using a <see cref="IWebSocketClient"/> in a <see cref="IWebSocketServer"/> method that it does not belong to.
/// </summary>
public sealed class ClientMissingException : WebSocketException
{
	/// <summary>
	/// The <see cref="IWebSocketServer"/> that was used.
	/// </summary>
	public IWebSocketServer Server { get; }
	/// <summary>
	/// The <see cref="IWebSocketClient"/> that was used.
	/// </summary>
	public IWebSocketClient Client { get; }

	public ClientMissingException( IWebSocketServer server, IWebSocketClient client ) : base( "Attempted to use a client that is not connected to this server" )
	{
		Server = server;
		Client = client;
	}
}
