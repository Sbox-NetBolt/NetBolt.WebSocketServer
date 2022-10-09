using System;

namespace NetBolt.WebSocket.Extensions;

/// <summary>
/// Extension class for <see cref="Span{T}"/> where T is a <see cref="byte"/>.
/// </summary>
internal static class SpanByteExtensions
{
	/// <summary>
	/// Sets a single byte.
	/// </summary>
	/// <param name="bytes">The bytes to modify.</param>
	/// <param name="offset">The offset into the <see ref="bytes"/> span to set the <see ref="value"/>.</param>
	/// <param name="value">The value to set.</param>
	internal static void Set( this Span<byte> bytes, int offset, byte value )
	{
		bytes[offset] = value;
	}

	/// <summary>
	/// Sets an unsigned short.
	/// </summary>
	/// <param name="bytes">The bytes to modify.</param>
	/// <param name="offset">The offset into the <see ref="bytes"/> span to set the <see ref="value"/>.</param>
	/// <param name="value">The value to set.</param>
	internal static void Set( this Span<byte> bytes, int offset, ushort value )
	{
		bytes[offset] = (byte)((value >> 8) & 255);
		bytes[offset + 1] = (byte)(value & 255);
	}

	/// <summary>
	/// Sets an unsigned long.
	/// </summary>
	/// <param name="bytes">The bytes to modify.</param>
	/// <param name="offset">The offset into the <see ref="bytes"/> span to set the <see ref="value"/>.</param>
	/// <param name="value">The value to set.</param>
	internal static void Set( this Span<byte> bytes, int offset, ulong value )
	{
		bytes[offset] = (byte)((value >> 56) & 255);
		bytes[offset + 1] = (byte)((value >> 48) & 255);
		bytes[offset + 2] = (byte)((value >> 40) & 255);
		bytes[offset + 3] = (byte)((value >> 32) & 255);
		bytes[offset + 4] = (byte)((value >> 24) & 255);
		bytes[offset + 5] = (byte)((value >> 16) & 255);
		bytes[offset + 6] = (byte)((value >> 8) & 255);
		bytes[offset + 7] = (byte)(value & 255);
	}
}
