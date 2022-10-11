using System;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Extensions;

namespace NetBolt.WebSocketServer.Extensions.Tests;

/// <summary>
/// A collection of tests covering <see cref="WebSocketDisconnectReasonExtensions"/>.
/// </summary>
public sealed class WebSocketDisconnectReasonExtensionsTests
{
	/// <summary>
	/// Verifies that all <see cref="WebSocketDisconnectReason"/>s have an associated <see cref="WebSocketCloseCode"/>.
	/// </summary>
	[Fact]
	public void AllReasonsHaveCloseCodes()
	{
		foreach ( var reason in Enum.GetValues<WebSocketDisconnectReason>() )
		{
			try
			{
				reason.GetCloseCode();
			}
			catch ( ArgumentOutOfRangeException )
			{
				Assert.Fail( $"{reason} does not have a {nameof(WebSocketCloseCode)}" );
			}
		}
	}
}
