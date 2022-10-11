using System;
using System.Text;
using NetBolt.WebSocket.Utility;

namespace NetBolt.WebSocketServer.Utility.Tests;

/// <summary>
/// A collection of tests covering <see cref="WebSocketHandshake"/>.
/// </summary>
public sealed class WebSocketHandshakeTests
{
	private const string Eol = "\r\n";
	
	/// <summary>
	/// Verifies that a full HTTP request for upgrade contains all headers.
	/// </summary>
	[Fact]
	public void RequestHeaders_Complete()
	{
		const string request = "GET / HTTP/1.1" + Eol +
		                       "Host: 127.0.0.1" + Eol +
		                       "Connection: upgrade" + Eol +
		                       "Upgrade: websocket" + Eol +
		                       "Sec-WebSocket-Key: x3JJHMbDL1EzLkh9GBhXDw==" + Eol +
		                       "Sec-WebSocket-Version: 13" + Eol +
		                       "Origin: 127.0.0.1" + Eol + Eol;
		var headers = WebSocketHandshake.GetRequestHeaders( request );
		
		Assert.True( headers.ContainsKey( "Host" ), "Missing \"Host\" header" );
		Assert.True( headers.ContainsKey( "Connection" ), "Missing \"Connection\" header" );
		Assert.True( headers.ContainsKey( "Upgrade" ), "Missing \"Upgrade\" header" );
		Assert.True( headers.ContainsKey( "Sec-WebSocket-Key" ), "Missing \"Sec-WebSocket-Key\" header" );
		Assert.True( headers.ContainsKey( "Sec-WebSocket-Version" ), "Missing \"Sec-WebSocket-Version\" header" );
		Assert.True( headers.ContainsKey( "Origin" ), "Missing \"Origin\" header" );
		
		Assert.Equal( "127.0.0.1", headers["Host"] );
		Assert.Equal( "upgrade", headers["Connection"] );
		Assert.Equal( "websocket", headers["Upgrade"] );
		Assert.Equal( "x3JJHMbDL1EzLkh9GBhXDw==", headers["Sec-WebSocket-Key"] );
		Assert.Equal( "13", headers["Sec-WebSocket-Version"] );
		Assert.Equal( "127.0.0.1", headers["Origin"] );
	}

	/// <summary>
	/// Verifies that an incomplete HTTP request for upgrade will not throw an exception and have an empty header dictionary.
	/// </summary>
	[Fact]
	public void RequestHeaders_Incomplete()
	{
		const string request = "GET / HTTP/1.1" + Eol + Eol;
		var headers = WebSocketHandshake.GetRequestHeaders( request );
		
		Assert.True( headers.Count == 0, $"Expected {nameof(headers)} to be empty" );
	}

	/// <summary>
	/// Verifies that an upgrade response is correct.
	/// </summary>
	[Fact]
	public void VerifyUpgradeResponse()
	{
		var response = WebSocketHandshake.GetUpgradeResponse( "x3JJHMbDL1EzLkh9GBhXDw==" ).Span;
		Span<byte> expected = Encoding.UTF8.GetBytes( "HTTP/1.1 101 Switching Protocols" + Eol +
		                                              "Connection: Upgrade" + Eol +
		                                              "Upgrade: websocket" + Eol +
		                                              "Sec-WebSocket-Accept: HSmrc0sMlYUkAGmm5OPpG2HaGWk=" + Eol +
		                                              Eol );
		
		Assert.True( expected.Length == response.Length, "Expected length to be the same" );
		for ( var i = 0; i < expected.Length; i++ )
			Assert.Equal( expected[i], response[i] );
	}
}
