using System;

namespace NetBolt.WebSocket.Exceptions;

/// <summary>
/// A base class for all exceptions in this library.
/// </summary>
public class WebSocketException : Exception
{
	public WebSocketException( string message ) : base( message )
	{
	}
}
