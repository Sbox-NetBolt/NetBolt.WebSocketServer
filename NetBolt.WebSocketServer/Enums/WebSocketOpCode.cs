namespace NetBolt.WebSocket.Enums;

/// <summary>
/// Represents a web socket message op code.
/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-5.2</remarks>
/// </summary>
public enum WebSocketOpCode
{
	/// <summary>
	/// Denotes a continuation frame.
	/// </summary>
	Continuation = 0,
	/// <summary>
	/// Denotes a text frame.
	/// </summary>
	Text = 1,
	/// <summary>
	/// Denotes a binary frame.
	/// </summary>
	Binary = 2,
	/// <summary>
	/// Reserved for non-control frame.
	/// </summary>
	NonControl0 = 3,
	/// <summary>
	/// Reserved for non-control frame.
	/// </summary>
	NonControl1 = 4,
	/// <summary>
	/// Reserved for non-control frame.
	/// </summary>
	NonControl2 = 5,
	/// <summary>
	/// Reserved for non-control frame.
	/// </summary>
	NonControl3 = 6,
	/// <summary>
	/// Reserved for non-control frame.
	/// </summary>
	NonControl4 = 7,
	/// <summary>
	/// Denotes a connection close.
	/// </summary>
	Close = 8,
	/// <summary>
	/// Denotes a ping.
	/// </summary>
	Ping = 9,
	/// <summary>
	/// Denotes a pong.
	/// </summary>
	Pong = 10,
	/// <summary>
	/// Reserved for control frame.
	/// </summary>
	Control0 = 11,
	/// <summary>
	/// Reserved for control frame.
	/// </summary>
	Control1 = 12,
	/// <summary>
	/// Reserved for control frame.
	/// </summary>
	Control2 = 13,
	/// <summary>
	/// Reserved for control frame.
	/// </summary>
	Control3 = 14,
	/// <summary>
	/// Reserved for control frame.
	/// </summary>
	Control4 = 15
}
