namespace NetBolt.WebSocket.Options;

/// <summary>
/// A read-only interface of <see cref="PingOptions"/>.
/// </summary>
public interface IReadOnlyPingOptions
{
	/// <summary>
	/// See <see cref="PingOptions.Enabled"/>.
	/// </summary>
	public bool Enabled { get; }
	/// <summary>
	/// See <see cref="PingOptions.Interval"/>.
	/// </summary>
	public int Interval { get; }
	/// <summary>
	/// See <see cref="PingOptions.Timeout"/>.
	/// </summary>
	public int Timeout { get; }
}
