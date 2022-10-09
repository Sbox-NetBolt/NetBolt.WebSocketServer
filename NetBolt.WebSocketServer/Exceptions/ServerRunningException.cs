namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// Thrown when trying to use logic that requires a <see cref="IWebSocketServer"/> to not be running and is.
/// </summary>
public sealed class ServerRunningException : WebSocketException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ServerRunningException"/> class.
	/// </summary>
	public ServerRunningException() : base( "Server is running" )
	{
	}
}
