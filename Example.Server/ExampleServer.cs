using System.Net.Sockets;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Options;

namespace Example.Server;

public sealed class ExampleServer : WebSocketServer
{
	public ExampleServer( IReadOnlyWebSocketServerOptions options ) : base( options )
	{
	}

	protected override IWebSocketClient CreateClient( TcpClient client )
	{
		return new ExampleClient( client, this );
	}
}
