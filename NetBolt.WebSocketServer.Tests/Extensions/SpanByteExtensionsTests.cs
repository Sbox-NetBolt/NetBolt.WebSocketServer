using System;
using NetBolt.WebSocket.Extensions;

namespace NetBolt.WebSocketServer.Extensions.Tests;

/// <summary>
/// A collection of tests covering <see cref="SpanByteExtensions"/>.
/// </summary>
public sealed class SpanByteExtensionsTests
{
	/// <summary>
	/// Tests <see cref="SpanByteExtensions.Set( Span{byte}, int, byte )"/>.
	/// </summary>
	[Theory]
	[InlineData( 0, 1 )]
	[InlineData( 1, 1 )]
	public void SetByte( int offset, byte value )
	{
		const int spanSize = 3;
		Assert.False( offset is < 0 or > spanSize - 1, "Tests inline data was setup incorrectly" );

		Span<byte> span = stackalloc byte[spanSize];
		span.Set( offset, value );

		for ( var i = 0; i < span.Length; i++ )
			Assert.Equal( i == offset ? value : 0, span[i] );
	}

	/// <summary>
	/// Tests <see cref="SpanByteExtensions.Set( Span{byte}, int, ushort )"/>.
	/// </summary>
	[Theory]
	[InlineData( 0, ushort.MaxValue )]
	[InlineData( 1, ushort.MaxValue )]
	public void SetUShort( int offset, ushort value )
	{
		const int spanSize = 4;
		Assert.False( offset is < 0 or > spanSize - 1, "Tests inline data was setup incorrectly" );
		
		Span<byte> span = stackalloc byte[spanSize];
		span.Set( offset, value );

		for ( var i = 0; i < span.Length; i++ )
		{
			if ( i == offset )
				Assert.Equal( (value >> 8) & 255, span[i] );
			else if ( i == offset + 1 )
				Assert.Equal( value & 255, span[i] );
			else
				Assert.Equal( 0, span[i] );
		}
	}

	/// <summary>
	/// Tests <see cref="SpanByteExtensions.Set( Span{byte}, int, ulong )"/>.
	/// </summary>
	[Theory]
	[InlineData( 0, ulong.MaxValue )]
	[InlineData( 1, ulong.MaxValue )]
	public void SetULong( int offset, ulong value )
	{
		const int spanSize = 10;
		Assert.False( offset is < 0 or > spanSize - 1, "Tests inline data was setup incorrectly" );
		
		Span<byte> span = stackalloc byte[spanSize];
		span.Set( offset, value );

		for ( var i = 0; i < span.Length; i++ )
		{
			if ( i == offset )
				Assert.Equal( (value >> 56) & 255, span[i] );
			else if ( i == offset + 1 )
				Assert.Equal( (value >> 48) & 255, span[i] );
			else if ( i == offset + 2 )
				Assert.Equal( (value >> 40) & 255, span[i] );
			else if ( i == offset + 3 )
				Assert.Equal( (value >> 32) & 255, span[i] );
			else if ( i == offset + 4 )
				Assert.Equal( (value >> 24) & 255, span[i] );
			else if ( i == offset + 5 )
				Assert.Equal( (value >> 16) & 255, span[i] );
			else if ( i == offset + 6 )
				Assert.Equal( (value >> 8) & 255, span[i] );
			else if ( i == offset + 7 )
				Assert.Equal( value & 255, span[i] );
			else
				Assert.Equal( 0, span[i] );
		}
	}
}
