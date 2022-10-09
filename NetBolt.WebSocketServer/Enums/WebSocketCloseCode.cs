namespace NetBolt.WebSocket.Enums;

/// <summary>
/// Represents a valid web socket close code.
/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-7.4.1</remarks>
/// </summary>
public enum WebSocketCloseCode
{
	/// <summary>
	/// Indicates a normal close.
	/// </summary>
	Normal = 1000,
	/// <summary>
	/// Indicates that an endpoint is "going away", such as a server going down.
	/// </summary>
	Shutdown = 1001,
	/// <summary>
	/// Indicates that an endpoint is terminating the connection due to a protocol error.
	/// </summary>
	ProtocolError = 1002,
	/// <summary>
	/// Indicates that an endpoint is terminating the connection because it has received a type of data it cannot accept.
	/// </summary>
	UnacceptableData = 1003,
	/// <summary>
	/// Reserved.
	/// </summary>
	Reserved = 1004,
	/// <summary>
	/// Reserved.
	/// </summary>
	ReservedMissing = 1005,
	/// <summary>
	/// Reserved.
	/// </summary>
	ReservedAbnormal = 1006,
	/// <summary>
	/// Indicates that an endpoint is terminating the connection because it has received data within a message that was not consistent with the type of the message.
	/// </summary>
	InconsistentData = 1007,
	/// <summary>
	/// Indicates that an endpoint is terminating the connection because it has received a message that violates its policy.
	/// </summary>
	PolicyViolation = 1008,
	/// <summary>
	/// Indicates that an endpoint is terminating the connection because it has received a message that is too big for it to process.
	/// </summary>
	MessageTooLarge = 1009,
	/// <summary>
	/// Indicates that an endpoint (client) is terminating the connection because it has expected the server to negotiate one or more extension, but the server didn't return them in the response message of the WebSocket handshake.
	/// </summary>
	ExtensionMissing = 1010,
	/// <summary>
	/// Indicates that an endpoint (client) is terminating the connection because it has expected the server to negotiate one or more extension, but the server didn't return them in the response message of the WebSocket handshake.
	/// </summary>
	UnexpectedError = 1011,
	/// <summary>
	///  Indicates that the connection was closed due to a failure to perform a TLS handshake.
	/// </summary>
	TlsFailure = 1015
}
