using System.Net;

namespace NetBolt.WebSocket.Options;

/// <summary>
/// A read-only interface of <see cref="WebSocketServerOptions"/>.
/// </summary>
public interface IReadOnlyWebSocketServerOptions
{
	/// <summary>
	/// See <see cref="WebSocketServerOptions.IpAddress"/>.
	/// </summary>
	public IPAddress IpAddress { get; }
	/// <summary>
	/// See <see cref="WebSocketServerOptions.Port"/>.
	/// </summary>
	public int Port { get; }
	/// <summary>
	/// See <see cref="WebSocketServerOptions.DisconnectPhrase"/>.
	/// </summary>
	public string DisconnectPhrase { get; }
	/// <summary>
	/// See <see cref="WebSocketServerOptions.AutoPing"/>.
	/// </summary>
	public IReadOnlyPingOptions ReadOnlyAutoPing { get; }
	/// <summary>
	/// See <see cref="WebSocketServerOptions.Messaging"/>.
	/// </summary>
	public IReadOnlyMessageOptions ReadOnlyMessaging { get; }
}
