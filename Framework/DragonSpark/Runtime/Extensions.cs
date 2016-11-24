using DragonSpark.Runtime.Data;
using DragonSpark.Sources.Parameterized;
using System;
using System.IO;

namespace DragonSpark.Runtime
{
	public static class Extensions
	{
		public static T Registered<T>( this IComposable<IDisposable> @this, T entry ) where T : IDisposable
		{
			@this.Add( entry );
			return entry;
		}

		public static T Registered<T>( this IComposable<object> @this, T entry ) 
		{
			@this.Add( entry );
			return entry;
		}

		// ReSharper disable once RedundantTypeArgumentsOfMethod
		public static T Load<T>( this ISerializer @this, string data ) => @this.Load<T>( new MemoryStream( Encoding.Default.GetFixed<string, byte>( data ) ) );
	}
}