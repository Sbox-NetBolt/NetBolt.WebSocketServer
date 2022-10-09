using System;
using System.Text;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Options;

namespace Example.Server;

public static class Program
{
	// Config constants
	private const bool UseDefault = true;

	private const string Ip = "127.0.0.1";
	private const int Port = 9987;
	private const string DisconnectPhrase = "disconnect";
	private const bool AutoPingEnabled = true;
	private const int AutoPingInterval = 20;
	private const int AutoPingTimeout = 5;
	private const int MaxMessageReceive = 32768;
	private const int MaxMessageSend = 65535;
	private const int MaxFrameSend = 16384;
	private const int MaxStackAllocDecode = 1024;
	private const int MaxStackAllocFrame = 1024;

	private static IWebSocketServer _server = null!;

	public static void Main( string[] args )
	{
		// Create config
		var config = UseDefault ? WebSocketServerOptions.Default : new WebSocketServerOptions()
			.WithIpAddress( Ip )
			.WithPort( Port )
			.WithDisconnectPhrase( DisconnectPhrase )
			.AutoPing.IsEnabled( AutoPingEnabled )
			.AutoPing.WithInterval( AutoPingInterval )
			.AutoPing.WithTimeout( AutoPingTimeout )
			.Messaging.WithMaxMessageReceiveBytes( MaxMessageReceive )
			.Messaging.WithMaxMessageSendBytes( MaxMessageSend )
			.Messaging.WithMaxFrameSendBytes( MaxFrameSend )
			.Messaging.WithMaxStackAllocDecodeBytes( MaxStackAllocDecode )
			.Messaging.WithMaxStackAllocFrameBytes( MaxStackAllocFrame );

		// Create and start server
		_server = new ExampleServer( config );
		_server.Start();
		Console.WriteLine( "Server started on {0}:{1}", Ip, Port );

		// Shut down server cleanly when exiting
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		// Wait while server is running and display options to the user
		var shouldExit = false;
		while ( !shouldExit )
		{
			Console.WriteLine( "Press [ENTER] to close" );
			Console.WriteLine( "Press 1 to send a text message to everyone" );
			Console.WriteLine( "Press 2 to send a binary message to everyone" );
			Console.WriteLine( "Press 3 to send a massive binary message to everyone" );
			Console.WriteLine( "Press 4 to disconnect all clients" );

			var result = Console.ReadLine();
			switch ( result )
			{
				case "1":
					_server.QueueSend( To.All( _server ), "Hello, World!" );
					break;
				case "2":
					_server.QueueSend( To.All( _server ), Encoding.UTF8.GetBytes( "Hello, World!" ) );
					break;
				case "3":
					_server.QueueSend( To.All( _server ), new byte[16384 * 2] );
					break;
				case "4":
					foreach ( var client in _server.Clients )
						_ = client.DisconnectAsync( WebSocketDisconnectReason.Requested, "Manual disconnect from the server" );
					break;
				default:
					shouldExit = true;
					break;
			}
		}

		// Shut down cleanly
		Console.WriteLine( "Shutting down..." );
		AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
		_server.StopAsync().Wait();
	}

	private static void OnProcessExit( object? sender, EventArgs eventArgs )
	{
		_server.StopAsync().Wait();
	}
}
