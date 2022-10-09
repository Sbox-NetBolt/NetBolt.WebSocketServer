using System;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace NetBolt.Client;

public class ClientGame : Game
{
	private new static ClientGame Current => (ClientGame)Game.Current;

	private WebSocket? _webSocket;
	private string _currentUri = string.Empty;

	private async Task ConnectAsync( string ip, int port, bool secure = false )
	{
		if ( _webSocket is not null )
			await DisconnectAsync();

		_webSocket = new WebSocket();
		_webSocket.OnDisconnected += WebSocketOnDisconnected;
		_webSocket.OnDataReceived += WebSocketOnDataReceived;
		_webSocket.OnMessageReceived += WebSocketOnMessageReceived;

		_currentUri = (secure ? "wss://" : "ws://") + ip + ":" + port;
		Log.Info( $"Connecting to {_currentUri}..." );
		await _webSocket.Connect( _currentUri );
		WebSocketOnConnected();
	}

	private async Task DisconnectAsync( bool clean = true )
	{
		if ( _webSocket is null || !_webSocket.IsConnected )
			return;

		Log.Info( $"Disconnecting from {_currentUri}..." );
		if ( clean )
			await _webSocket.CloseAsync( "disconnect" );
		else
			_webSocket.Dispose();

		_webSocket.OnDisconnected -= WebSocketOnDisconnected;
		_webSocket.OnDataReceived -= WebSocketOnDataReceived;
		_webSocket.OnMessageReceived -= WebSocketOnMessageReceived;
		_webSocket = null;
	}

	private void WebSocketOnConnected()
	{
		Log.Info( $"Connected to {_currentUri}" );
	}

	private void WebSocketOnDisconnected( int status, string reason )
	{
		Log.Info( $"Disconnected from {_currentUri}. Reason: {reason}, Status Code: {status}" );
	}

	private void WebSocketOnDataReceived( Span<byte> data )
	{
		Log.Info( $"Received {data.Length} bytes" );
	}

	private void WebSocketOnMessageReceived( string message )
	{
		Log.Info( $"Received message: {message}" );
	}

	[ConCmd.Client( "connect_to_server" )]
	public static void ConnectToServer( string ip, int port, bool secure = false )
	{
		_ = Current.ConnectAsync( ip, port, secure );
	}

	[ConCmd.Client( "disconnect_from_server" )]
	public static void DisconnectFromServer()
	{
		_ = Current.DisconnectAsync();
	}

	[ConCmd.Client( "timeout_from_server" )]
	public static void TimeoutFromServer()
	{
		_ = Current.DisconnectAsync( false );
	}

	[ConCmd.Client( "send_message" )]
	public static async void SendMessage( string message )
	{
		if ( Current._webSocket is null || !Current._webSocket.IsConnected )
			return;

		await Current._webSocket.Send( message );
	}

	[ConCmd.Client( "spam_message" )]
	public static async void SpamMessage( string message, int amount, int delay )
	{
		if ( Current._webSocket is null || !Current._webSocket.IsConnected )
			return;

		for ( var i = 0; i < amount; i++ )
		{
			await Current._webSocket.Send( message );
			await System.Threading.Tasks.Task.Delay( delay );
		}
	}

	[ConCmd.Client( "send_message_bytes" )]
	public static async void SendMessageBytes( string message )
	{
		if ( Current._webSocket is null || !Current._webSocket.IsConnected )
			return;

		await Current._webSocket.Send( Encoding.UTF8.GetBytes( message ) );
	}

	[ConCmd.Client( "spam_message_bytes" )]
	public static async void SpamMessageBytes( string message, int amount, int delay )
	{
		if ( Current._webSocket is null || !Current._webSocket.IsConnected )
			return;

		var bytes = Encoding.UTF8.GetBytes( message );
		for ( var i = 0; i < amount; i++ )
		{
			await Current._webSocket.Send( bytes );
			await System.Threading.Tasks.Task.Delay( delay );
		}
	}
}
