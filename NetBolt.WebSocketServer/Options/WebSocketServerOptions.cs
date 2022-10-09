using System.Net;
using NetBolt.WebSocket.Enums;

namespace NetBolt.WebSocket.Options;

/// <summary>
/// A configuration class for a web socket server.
/// </summary>
public sealed class WebSocketServerOptions : IReadOnlyWebSocketServerOptions
{
	/// <summary>
	/// Creates a default instance of <see cref="WebSocketServerOptions"/>.
	/// </summary>
	public static WebSocketServerOptions Default => new();
	
	/// <summary>
	/// The IP address the web socket server should bind to.
	/// </summary>
	public IPAddress IpAddress { get; set; } = IPAddress.Any;
	/// <summary>
	/// The port the web socket server should listen on.
	/// </summary>
	public int Port { get; set; } = 1000;
	/// <summary>
	/// The phrase a client must send in a <see cref="WebSocketOpCode.Text"/> message to cleanly disconnect.
	/// </summary>
	public string DisconnectPhrase { get; set; } = "disconnect";

	/// <summary>
	/// Ping options for automatic pinging of clients in the server.
	/// </summary>
	public PingOptions AutoPing { get; }
	public IReadOnlyPingOptions ReadOnlyAutoPing => AutoPing;

	/// <summary>
	/// Options for limiting messages and their construction.
	/// </summary>
	public MessageOptions Messaging { get; }
	public IReadOnlyMessageOptions ReadOnlyMessaging => Messaging;

	public WebSocketServerOptions()
	{
		AutoPing = new PingOptions( this );
		Messaging = new MessageOptions( this );
	}

	/// <summary>
	/// Sets the <see cref="IpAddress"/> option.
	/// </summary>
	/// <param name="ipAddress">The IP address to bind to.</param>
	/// <returns>This config instance.</returns>
	public WebSocketServerOptions WithIpAddress( IPAddress ipAddress )
	{
		IpAddress = ipAddress;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="IpAddress"/> option from a parsed <see cref="IPAddress"/>.
	/// </summary>
	/// <param name="ipAddress">The string IP address to bind to.</param>
	/// <returns>This config instance.</returns>
	public WebSocketServerOptions WithIpAddress( string ipAddress )
	{
		IpAddress = IPAddress.Parse( ipAddress );
		return this;
	}

	/// <summary>
	/// Sets the <see cref="Port"/> option.
	/// </summary>
	/// <param name="port">The port to listen on.</param>
	/// <returns>This config instance.</returns>
	public WebSocketServerOptions WithPort( int port )
	{
		Port = port;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="DisconnectPhrase"/> option.
	/// </summary>
	/// <param name="disconnectPhrase">The phrase a client must send in a <see cref="WebSocketOpCode.Text"/> message to cleanly disconnect.</param>
	/// <returns>This config instance.</returns>
	public WebSocketServerOptions WithDisconnectPhrase( string disconnectPhrase )
	{
		DisconnectPhrase = disconnectPhrase;
		return this;
	}
}
