using System.Diagnostics.CodeAnalysis;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Configuration
{
	[SuppressMessage( "ReSharper", "PossibleInfiniteInheritance" )]
	public class ConfigurationSource<T> : SuppliedAndExportedItems<ITransformer<T>>
	{
		public ConfigurationSource( params ITransformer<T>[] configurators ) : base( configurators ) {}
	}
}