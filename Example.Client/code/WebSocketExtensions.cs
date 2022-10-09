using System.Threading.Tasks;
using Sandbox;

namespace NetBolt.Client;

/// <summary>
/// Extension class for <see cref="WebSocket"/>.
/// </summary>
public static class WebSocketExtensions
{
	/// <summary>
	/// Sends a disconnect message to the server then disposes the web socket.
	/// </summary>
	/// <param name="socket">The socket to close.</param>
	/// <param name="disconnectPhrase">The phrase to send to the server to signify the disconnect.</param>
	public static async Task CloseAsync( this WebSocket socket, string disconnectPhrase )
	{
		if ( !socket.IsConnected )
			return;

		await socket.Send( disconnectPhrase );
		await Task.Delay( 1 );
		socket.Dispose();
	}
}
