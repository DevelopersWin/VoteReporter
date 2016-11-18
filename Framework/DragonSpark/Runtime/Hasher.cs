using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Sources.Scopes;
using JetBrains.Annotations;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DragonSpark.Runtime
{
	public interface IHasher : IAlteration<ImmutableArray<byte>> {}
	public sealed class Hasher : ParameterizedScope<ImmutableArray<byte>, ImmutableArray<byte>>, IHasher
	{
		public static Hasher Default { get; } = new Hasher();
		Hasher() : base( bytes => Encoding.Default.Get( bytes ).GetHashCode().With( hash => ImmutableArray.Create( (byte)(hash >> 24), (byte)(hash >> 16), (byte)(hash >> 8), (byte)hash ) ) ) {}
	}

	public sealed class TextHasher : EqualityReferenceCache<string, string>
	{
		public static TextHasher Default { get; } = new TextHasher();
		TextHasher() : base( Implementation.Instance.Get ) {}

		sealed class Implementation : AlterationBase<string>
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( Hasher.Default, Encoding.Default ) {}

			readonly IHasher hasher;
			readonly Encoding encoding;

			[UsedImplicitly]
			public Implementation( IHasher hasher, Encoding encoding )
			{
				this.hasher = hasher;
				this.encoding = encoding;
			}

			public override string Get( string parameter ) =>
				hasher
					.Get( encoding.Get( parameter ) )
					.Aggregate( new StringBuilder(), ( builder, current ) => builder.Append( current.ToString( "X2" ) ) )
					.ToString();
		}
	}
}
