using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Extensions;

namespace NetBolt.WebSocket.Utility;

/// <summary>
/// Utility class to handle web socket frames being sent to and from the server.
/// <remarks>https://www.rfc-editor.org/rfc/rfc6455#section-5.2</remarks>
/// </summary>
public sealed class WebSocketFrame
{
	/// <summary>
	/// The maximum size the frame header can be.
	/// </summary>
	private const int MaxHeaderSize = 10;

	/// <summary>
	/// Whether or not this frame has the "FIN" bit set.
	/// </summary>
	public bool Finished { get; }
	/// <summary>
	/// Whether or not this frame has the "MASK" bit set.
	/// </summary>
	public bool Masked { get; }
	/// <summary>
	/// The <see cref="WebSocketOpCode"/> this frame represents.
	/// </summary>
	public WebSocketOpCode OpCode { get; }
	/// <summary>
	/// The length of the data payload.
	/// </summary>
	public ulong DataLength { get; }
	/// <summary>
	/// The data payload.
	/// </summary>
	public ReadOnlyMemory<byte> Data { get; }

	private WebSocketFrame( bool finished, bool masked, WebSocketOpCode opCode, ulong dataLength,
		ReadOnlyMemory<byte> data )
	{
		Finished = finished;
		Masked = masked;
		OpCode = opCode;
		DataLength = dataLength;
		Data = data;
	}

	/// <summary>
	/// Gets the amount of bytes this frame will take to contain.
	/// </summary>
	/// <returns></returns>
	[Pure]
	public int GetByteSize()
	{
		return GetFrameSize( Data.Span );
	}

	/// <summary>
	/// Serializes the frame to an array of bytes.
	/// </summary>
	/// <param name="maxStackAllocFrameBytes">The maximum amount of bytes that can be allocated on the stack for building the frame.</param>
	/// <returns>The serialized frame.</returns>
	/// <exception cref="InvalidOperationException">Thrown when getting bytes of a message with <see cref="Masked"/> being true.</exception>
	[Pure]
	public ReadOnlyMemory<byte> GetBytes( int maxStackAllocFrameBytes = int.MaxValue )
	{
		if ( Masked )
			throw new InvalidOperationException( "A server does not mask messages" );

		if ( DataLength == 0 )
			return new byte[] { (byte)((Finished ? 128 : 0) + OpCode), 0 };

		var extraBytes = 1 + GetLengthSize( Data.Span );
		var totalSize = (int)DataLength + extraBytes;
		var frameBytesSpan = totalSize <= maxStackAllocFrameBytes ? stackalloc byte[totalSize] : new byte[totalSize];
		frameBytesSpan[0] = (byte)((Finished ? 128 : 0) + OpCode);
		switch ( DataLength )
		{
			case <= 125:
				frameBytesSpan.Set( 1, (byte)DataLength );
				break;
			case <= 65535:
				frameBytesSpan[1] = 126;
				frameBytesSpan.Set( 2, (ushort)DataLength );
				break;
			default:
				frameBytesSpan[1] = 127;
				frameBytesSpan.Set( 2, DataLength );
				break;
		}

		var frameData = frameBytesSpan.Slice( extraBytes, (int)DataLength );
		Data.Span.CopyTo( frameData );

		return frameBytesSpan.ToArray();
	}

	/// <summary>
	/// Formats a <see cref="WebSocketOpCode.Close"/> message data.
	/// </summary>
	/// <param name="reason">The reason for the disconnect.</param>
	/// <param name="wordReason">A string version of <see ref="reason"/>.</param>
	/// <returns>The formatted close message data.</returns>
	[Pure]
	public static ReadOnlyMemory<byte> FormatCloseData( WebSocketDisconnectReason reason, string wordReason = "" )
	{
		var strBytesSpan = string.IsNullOrWhiteSpace( wordReason )
			? Span<byte>.Empty
			: Encoding.UTF8.GetBytes( wordReason );

		var closeCode = (ushort)reason.GetCloseCode();
		Span<byte> closeDataBytesSpan = new byte[strBytesSpan.Length + 2];
		closeDataBytesSpan.Set( 0, closeCode );

		if ( strBytesSpan.Length <= 0 )
			return closeDataBytesSpan.ToArray();

		var closeDataReasonSpan = closeDataBytesSpan.Slice( 2, closeDataBytesSpan.Length - 2 );
		strBytesSpan.CopyTo( closeDataReasonSpan );

		return closeDataBytesSpan.ToArray();
	}

	/// <summary>
	/// Creates a frame for sending to a client.
	/// </summary>
	/// <param name="opCode">The <see cref="WebSocketOpCode"/> associated with this frame.</param>
	/// <param name="data">The data payload of the frame.</param>
	/// <param name="finished">Whether or not this frame has the "FIN" bit set.</param>
	/// <returns>The finished frame.</returns>
	[Pure]
	public static WebSocketFrame Frame( WebSocketOpCode opCode, ReadOnlySpan<byte> data, bool finished = true )
	{
		var dataBytesLength = (ulong)data.Length;
		return dataBytesLength == 0
			? new WebSocketFrame( finished, false, opCode, 0, ReadOnlyMemory<byte>.Empty )
			: new WebSocketFrame( finished, false, opCode, dataBytesLength, data.ToArray() );
	}

	/// <summary>
	/// Creates the required frames for the data and op code provided.
	/// </summary>
	/// <param name="opCode">The <see cref="WebSocketOpCode"/> associated with this message.</param>
	/// <param name="data">The total data payload.</param>
	/// <param name="maxBytesPerFrame">The maximum amount of bytes that can be sent to a client in a single frame.</param>
	/// <returns>An iterator of all the frames.</returns>
	[Pure]
	public static IEnumerable<WebSocketFrame> FrameMulti( WebSocketOpCode opCode, ReadOnlyMemory<byte> data, int maxBytesPerFrame = int.MaxValue )
	{
		var numFrames = GetNumFramesRequired( data.Span, maxBytesPerFrame );
		if ( numFrames == 1 )
		{
			yield return Frame( opCode, data.Span );
			yield break;
		}

		yield return Frame( opCode, data.Span[..(maxBytesPerFrame - MaxHeaderSize)], false );

		for ( var i = 1; i < numFrames - 1; i++ )
			yield return Frame( WebSocketOpCode.Continuation, data.Span.Slice( (maxBytesPerFrame - MaxHeaderSize) * i, maxBytesPerFrame - MaxHeaderSize ), false );

		yield return Frame( WebSocketOpCode.Continuation, data.Span[((maxBytesPerFrame - MaxHeaderSize) * (numFrames - 1))..] );
	}

	/// <summary>
	/// Gets the amount of bytes needed to make a header for the data.
	/// </summary>
	/// <param name="data">The data to build a header for.</param>
	/// <returns>The amount of bytes needed to make a header for the data.</returns>
	[Pure]
	public static int GetHeaderSize( ReadOnlySpan<byte> data )
	{
		// 1 to contain the "FIN" bit, extension bits, and op code. Rest is the payload length.
		return 1 + GetLengthSize( data );
	}

	/// <summary>
	/// Gets the amount of bytes needed to contain the data payload length.
	/// </summary>
	/// <param name="data">The data to gets the length of.</param>
	/// <returns>The amount of bytes needed to contain the data payload length.</returns>
	[Pure]
	public static int GetLengthSize( ReadOnlySpan<byte> data )
	{
		return data.Length switch
		{
			<= 125 => 1,
			// 1 for the marker and 2 for the size.
			<= 65535 => 3,
			// 1 for the marker and 2 for the size.
			_ => 9
		};
	}

	/// <summary>
	/// Gets the number of frames required to contain all the data without exceeding a maximum amount of bytes per frame.
	/// </summary>
	/// <param name="data">The data to contain in frames/</param>
	/// <param name="maxBytesPerFrame">The maximum amount of bytes that can be sent to a client in a single frame.</param>
	/// <returns>The number of frames required to contain all of the data without exceeding the maximum amount of bytes per frame.</returns>
	[Pure]
	public static int GetNumFramesRequired( ReadOnlySpan<byte> data, int maxBytesPerFrame )
	{
		return (int)Math.Ceiling( Math.Clamp( (data.Length + MaxHeaderSize) / (float)maxBytesPerFrame, 1, float.MaxValue ) );
	}

	/// <summary>
	/// Gets the amount of bytes required to contain the frame with its data.
	/// </summary>
	/// <param name="data">The data to include in the frame/</param>
	/// <returns>The amount of bytes required to contain the frame with its data.</returns>
	[Pure]
	public static int GetFrameSize( ReadOnlySpan<byte> data )
	{
		return GetHeaderSize( data ) + data.Length;
	}

	/// <summary>
	/// Gets the amount of bytes required to contain all the frames that make up a message with its data.
	/// </summary>
	/// <param name="data">The data to send.</param>
	/// <param name="maxBytesPerFrame">The maximum amount of bytes that can be sent to a client in a single frame.</param>
	/// <returns>The amount of bytes required to contain all the frames that make up a message with its data.</returns>
	[Pure]
	public static int GetTotalMessageSize( ReadOnlySpan<byte> data, int maxBytesPerFrame )
	{
		var numFrames = GetNumFramesRequired( data, maxBytesPerFrame );
		var lastSpan = data[((maxBytesPerFrame - MaxHeaderSize) * (numFrames - 1))..];
		return numFrames * maxBytesPerFrame + GetHeaderSize( lastSpan ) + lastSpan.Length;
	}

	/// <summary>
	/// Parses a frame received from a client.
	/// </summary>
	/// <param name="bytes">The bytes containing the clients message.</param>
	/// <param name="maxStackAllocDecodeBytes">The maximum amount of bytes that can be allocated on the stack for decoding client message frames.</param>
	/// <returns>The parsed client frame.</returns>
	[Pure]
	public static WebSocketFrame ParseClientFrame( ReadOnlySpan<byte> bytes,
		int maxStackAllocDecodeBytes = int.MaxValue )
	{
		var finished = (bytes[0] & 0b10000000) != 0;
		var masked = (bytes[1] & 0b10000000) != 0;
		var opCode = (WebSocketOpCode)(bytes[0] & 0b00001111);

		var offset = 2UL;
		var dataLength = bytes[1] & 0b01111111UL;

		switch ( dataLength )
		{
			case 126:
				dataLength = BitConverter.ToUInt16( new[] { bytes[3], bytes[2] }, 0 );
				offset = 4;
				break;
			case 127:
				dataLength =
					BitConverter.ToUInt64(
						new[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0 );
				offset = MaxHeaderSize;
				break;
		}

		var data = masked
			? Decode( bytes, (int)dataLength, (int)offset, maxStackAllocDecodeBytes )
			: bytes[(int)offset..].ToArray();
		return new WebSocketFrame( finished, masked, opCode, dataLength, data );
	}

	/// <summary>
	/// Decodes a clients data payload.
	/// </summary>
	/// <param name="bytes">The bytes that contains the whole client frame.</param>
	/// <param name="dataLength">The amount of data that is contained in the message.</param>
	/// <param name="offset">The offset at which to start the decoding process.</param>
	/// <param name="maxStackAllocDecodeBytes">The maximum amount of bytes that can be allocated on the stack for decoding the data.</param>
	/// <returns>The decoded data payload.</returns>
	[Pure]
	private static ReadOnlyMemory<byte> Decode( ReadOnlySpan<byte> bytes, int dataLength, int offset,
		int maxStackAllocDecodeBytes = int.MaxValue )
	{
		var decoded = bytes.Length <= maxStackAllocDecodeBytes ? stackalloc byte[dataLength] : new byte[dataLength];
		var masks = bytes.Slice( offset, 4 );
		offset += 4;

		for ( var i = 0; i < dataLength; ++i )
			decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

		return decoded.ToArray();
	}
}
