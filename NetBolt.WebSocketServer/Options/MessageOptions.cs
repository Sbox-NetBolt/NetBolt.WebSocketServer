namespace NetBolt.WebSocket.Options;

/// <summary>
/// A collection of options relating to message sending/receiving.
/// </summary>
public sealed class MessageOptions : IReadOnlyMessageOptions
{
	/// <summary>
	/// The maximum amount of bytes that can be received from a client for a complete message.
	/// </summary>
	public int MaxMessageReceiveBytes { get; set; } = 32768;
	/// <summary>
	/// The maximum amount of bytes that can be sent to a client for a complete message.
	/// </summary>
	public int MaxMessageSendBytes { get; set; } = 65535;
	/// <summary>
	/// The maximum amount of bytes that can be sent to a client in a single message frame.
	/// </summary>
	public int MaxFrameSendBytes { get; set; } = 16384;
	/// <summary>
	/// The maximum amount of bytes that can be allocated on the stack for decoding client message frames.
	/// </summary>
	public int MaxStackAllocDecodeBytes { get; set; } = 1024;
	/// <summary>
	/// The maximum amount of bytes that can be allocated on the stack for building message frames.
	/// </summary>
	public int MaxStackAllocFrameBytes { get; set; } = 1024;

	/// <summary>
	/// The parent options this instance is a part of.
	/// </summary>
	private readonly WebSocketServerOptions _options;

	internal MessageOptions( WebSocketServerOptions options )
	{
		_options = options;
	}

	/// <summary>
	/// Sets the <see cref="MaxMessageReceiveBytes"/> option.
	/// </summary>
	/// <param name="maxMessageReceiveBytes">The maximum amount of bytes that can be received from a client for a complete message.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithMaxMessageReceiveBytes( int maxMessageReceiveBytes )
	{
		MaxMessageReceiveBytes = maxMessageReceiveBytes;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="MaxMessageSendBytes"/> option.
	/// </summary>
	/// <param name="maxMessageSendBytes">The maximum amount of bytes that can be sent to a client for a complete message.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithMaxMessageSendBytes( int maxMessageSendBytes )
	{
		MaxMessageSendBytes = maxMessageSendBytes;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="MaxFrameSendBytes"/> option.
	/// </summary>
	/// <param name="maxFrameSendBytes">The maximum amount of bytes that can be sent to a client in a single message frame.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithMaxFrameSendBytes( int maxFrameSendBytes )
	{
		MaxFrameSendBytes = maxFrameSendBytes;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="MaxStackAllocDecodeBytes"/> option.
	/// </summary>
	/// <param name="maxStackAllocDecodeBytes">The maximum amount of bytes that can be allocated on the stack for decoding client message frames.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithMaxStackAllocDecodeBytes( int maxStackAllocDecodeBytes )
	{
		MaxStackAllocDecodeBytes = maxStackAllocDecodeBytes;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="MaxStackAllocFrameBytes"/> option.
	/// </summary>
	/// <param name="maxStackAllocFrameBytes">The maximum amount of bytes that can be allocated on the stack for building message frames.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithMaxStackAllocFrameBytes( int maxStackAllocFrameBytes )
	{
		MaxStackAllocFrameBytes = maxStackAllocFrameBytes;
		return _options;
	}
}
