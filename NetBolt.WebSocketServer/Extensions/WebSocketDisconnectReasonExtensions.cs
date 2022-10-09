using System;
using NetBolt.WebSocket.Enums;

namespace NetBolt.WebSocket.Extensions;

/// <summary>
/// Extension class for <see cref="WebSocketDisconnectReason"/>.
/// </summary>
internal static class WebSocketDisconnectReasonExtensions
{
	/// <summary>
	/// Converts a <see cref="WebSocketDisconnectReason"/> to a valid <see cref="WebSocketCloseCode"/>.
	/// </summary>
	/// <param name="reason">The reason to convert.</param>
	/// <param name="error">The error that is associated with <see cref="reason"/>.</param>
	/// <returns>The converted close code.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="reason"/> passed is invalid.</exception>
	internal static WebSocketCloseCode GetCloseCode( this WebSocketDisconnectReason reason, WebSocketError? error = null )
	{
		switch ( reason )
		{
			case WebSocketDisconnectReason.None:
				return WebSocketCloseCode.Normal;
			case WebSocketDisconnectReason.Error:
				switch ( error )
				{
					case WebSocketError.MessageTooLarge:
						return WebSocketCloseCode.MessageTooLarge;
					case WebSocketError.MessageUnfinished:
					case WebSocketError.MissingMask:
					case WebSocketError.UpgradeFail:
						return WebSocketCloseCode.ProtocolError;
					case WebSocketError.HandlingException:
					case WebSocketError.StreamDisposed:
					case WebSocketError.WriteError:
					case null:
						return WebSocketCloseCode.UnexpectedError;
					default:
						throw new ArgumentOutOfRangeException( nameof( error ), error, null );
				}
			case WebSocketDisconnectReason.Requested:
				return WebSocketCloseCode.Normal;
			case WebSocketDisconnectReason.ServerShutdown:
				return WebSocketCloseCode.Shutdown;
			case WebSocketDisconnectReason.Timeout:
				return WebSocketCloseCode.ProtocolError;
			default:
				throw new ArgumentOutOfRangeException( nameof( reason ), reason, null );
		}
	}
}
