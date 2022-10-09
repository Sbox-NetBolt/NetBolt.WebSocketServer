using System;

namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// A base class for all exceptions in this library.
/// </summary>
public class WebSocketException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WebSocketException"/> class with a message that describes the error.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public WebSocketException( string message ) : base( message )
	{
	}
}
