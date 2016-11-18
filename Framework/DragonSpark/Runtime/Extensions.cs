using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DragonSpark.Runtime
{
	public sealed class Encoding : SingletonScope<System.Text.Encoding>, IParameterizedSource<string, ImmutableArray<byte>>, IParameterizedSource<ImmutableArray<byte>, string>
	{
		public static Encoding Default { get; } = new Encoding();
		Encoding() : base( () => System.Text.Encoding.UTF8 ) {}


		public ImmutableArray<byte> Get( string parameter ) => Get().GetBytes( parameter ).ToImmutableArray();

		public string Get( ImmutableArray<byte> parameter )
		{
			var bytes = parameter.ToArray();
			var result = Get().GetString( bytes, 0, bytes.Length );
			return result;
		}
	}

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