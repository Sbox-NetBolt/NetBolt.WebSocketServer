using System;
using System.Net.Sockets;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Enums;

namespace Example.Server;

public sealed class ExampleClient : WebSocketClient
{
	public ExampleClient( TcpClient socket, IWebSocketServer server ) : base( socket, server )
	{
	}

	protected override void OnConnected()
	{
		base.OnConnected();

		Console.WriteLine( $"{this} has connected" );
	}

	protected override void OnUpgraded()
	{
		base.OnUpgraded();

		Console.WriteLine( $"{this} has upgraded to web socket protocol" );
	}

	protected override void OnDisconnected( WebSocketDisconnectReason reason, WebSocketError? error )
	{
		base.OnDisconnected( reason, error );

		Console.WriteLine( $"{this} has disconnected for reason: {reason}" );
		if ( reason == WebSocketDisconnectReason.Error )
			Console.WriteLine( $"\tThe error was: {error}" );
	}

	protected override void OnData( ReadOnlySpan<byte> bytes )
	{
		base.OnData( bytes );

		Console.WriteLine( $"{this} sent {bytes.Length} bytes" );
	}

	protected override void OnMessage( string message )
	{
		base.OnMessage( message );

		Console.WriteLine( $"{this} sent \"{message}\"" );
	}
}
