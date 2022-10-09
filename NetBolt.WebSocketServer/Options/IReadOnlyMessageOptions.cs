namespace NetBolt.WebSocket.Options;

/// <summary>
/// A read-only interface of <see cref="MessageOptions"/>.
/// </summary>
public interface IReadOnlyMessageOptions
{
	/// <summary>
	/// See <see cref="MessageOptions.MaxMessageReceiveBytes"/>.
	/// </summary>
	public int MaxMessageReceiveBytes { get; }
	/// <summary>
	/// See <see cref="MessageOptions.MaxMessageSendBytes"/>.
	/// </summary>
	public int MaxMessageSendBytes { get; }
	/// <summary>
	/// See <see cref="MessageOptions.MaxFrameSendBytes"/>.
	/// </summary>
	public int MaxFrameSendBytes { get; }
	/// <summary>
	/// See <see cref="MessageOptions.MaxStackAllocDecodeBytes"/>.
	/// </summary>
	public int MaxStackAllocDecodeBytes { get; }
	/// <summary>
	/// See <see cref="MessageOptions.MaxStackAllocFrameBytes"/>.
	/// </summary>
	public int MaxStackAllocFrameBytes { get; }
}
