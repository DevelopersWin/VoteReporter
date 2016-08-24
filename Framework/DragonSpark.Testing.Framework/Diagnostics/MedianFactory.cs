using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	sealed class MedianFactory : ParameterizedSourceBase<ImmutableArray<long>, long>
	{
		public static MedianFactory DefaultNested { get; } = new MedianFactory();

		public override long Get( ImmutableArray<long> parameter )
		{
			var length = parameter.Length;
			var middle = length / 2;
			var ordered = parameter.ToArray().OrderBy( i => i ).ToArray();
			var median = ordered.ElementAt( middle ) + ordered.ElementAt( ( length - 1 ) / 2 );
			var result = median / 2;
			return result;
		}
	}
}