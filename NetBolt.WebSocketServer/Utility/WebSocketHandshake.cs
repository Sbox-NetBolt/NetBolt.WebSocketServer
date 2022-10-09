using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NetBolt.WebSocket.Utility;

/// <summary>
/// Utility class to hold logic relating to the initial web socket handshake.
/// </summary>
internal static class WebSocketHandshake
{
	/// <summary>
	/// The character sequence to end a line.
	/// </summary>
	private const string Eol = "\r\n";
	/// <summary>
	/// The base string of the response to be sent to the client to upgrade them to the web socket protocol.
	/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-1.3</remarks>
	/// </summary>
	private const string ResponseStringBase = "HTTP/1.1 101 Switching Protocols" + Eol +
											  "Connection: Upgrade" + Eol +
											  "Upgrade: websocket" + Eol +
											  "Sec-WebSocket-Accept: __KEY__" + Eol +
											  Eol;
	/// <summary>
	/// The magic string Guid for the opening handshake.
	/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-1.3</remarks>
	/// </summary>
	private const string RfcGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

	/// <summary>
	/// Gets the key-value headers of an HTTP request.
	/// </summary>
	/// <param name="request">The request string to get headers from.</param>
	/// <returns>The key-values headers of an HTTP request.</returns>
	[Pure]
	internal static IReadOnlyDictionary<string, string> GetRequestHeaders( string request )
	{
		var headers = new Dictionary<string, string>();
		var lines = request.Split( Eol ).Skip( 1 ).SkipLast( 1 );

		foreach ( var line in lines )
		{
			if ( string.IsNullOrWhiteSpace( line ) )
				continue;

			var seperatorIndex = line.IndexOf( ':' );
			var key = line[..seperatorIndex];
			var value = line.Substring( seperatorIndex + 2, line.Length - (seperatorIndex + 2) );
			headers.Add( key, value );
		}

		return headers;
	}

	/// <summary>
	/// Builds an upgrade response from a client http request.
	/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-1.3</remarks>
	/// </summary>
	/// <param name="webSocketKey">The key the client requested an upgrade with.</param>
	/// <returns>The upgrade response for the client.</returns>
	[Pure]
	internal static ReadOnlyMemory<byte> GetUpgradeResponse( string webSocketKey )
	{
		var key = webSocketKey + RfcGuid;
		var accept = Convert.ToBase64String( SHA1.Create().ComputeHash( Encoding.UTF8.GetBytes( key ) ) );

		return Encoding.UTF8.GetBytes( ResponseStringBase.Replace( "__KEY__", accept ) );
	}
}
