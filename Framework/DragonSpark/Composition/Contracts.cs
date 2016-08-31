using DragonSpark.Sources.Parameterized.Caching;
using System.Composition.Hosting.Core;
using System.Linq;

namespace DragonSpark.Composition
{
	public sealed class Contracts : FactoryCache<CompositionContract, string>
	{
		public static Contracts Default { get; } = new Contracts();
		Contracts() {}

		protected override string Create( CompositionContract parameter )
		{
			object sharingBoundaryMetadata = null;
			var result = parameter.MetadataConstraints?.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ?? false ? (string)sharingBoundaryMetadata : null;
			return result;
		}
	}
}