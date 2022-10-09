namespace NetBolt.WebSocket.Options;

/// <summary>
/// A collection of options relating to automated pinging of clients.
/// </summary>
public sealed class PingOptions : IReadOnlyPingOptions
{
	/// <summary>
	/// Whether or not pinging is enabled.
	/// </summary>
	public bool Enabled { get; set; }
	/// <summary>
	/// The interval in seconds between sending each ping.
	/// </summary>
	public int Interval { get; set; }
	/// <summary>
	/// The time in seconds to wait for a pong before disconnecting the client due to timeout.
	/// </summary>
	public int Timeout { get; set; }

	/// <summary>
	/// The parent options this instance is a part of.
	/// </summary>
	private readonly WebSocketServerOptions _options;

	internal PingOptions( WebSocketServerOptions options )
	{
		_options = options;
	}

	/// <summary>
	/// Sets the <see cref="Enabled"/> option.
	/// </summary>
	/// <param name="enabled">Whether or not pinging is enabled.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions IsEnabled( bool enabled )
	{
		Enabled = enabled;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="Interval"/> option.
	/// </summary>
	/// <param name="pingInterval">The interval in seconds between sending each ping.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithInterval( int pingInterval )
	{
		Interval = pingInterval;
		return _options;
	}

	/// <summary>
	/// Sets the <see cref="Timeout"/> option.
	/// </summary>
	/// <param name="pingTimeout">The time in seconds to wait for a pong before disconnecting the client due to timeout.</param>
	/// <returns>The parent options instance.</returns>
	public WebSocketServerOptions WithTimeout( int pingTimeout )
	{
		Timeout = pingTimeout;
		return _options;
	}
}
