using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;

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
}