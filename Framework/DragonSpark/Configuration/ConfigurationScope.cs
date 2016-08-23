using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Configuration
{
	[SuppressMessage( "ReSharper", "PossibleInfiniteInheritance" )]
	public class ConfigurationScope<T> : Scope<ImmutableArray<ITransformer<T>>>, IConfigurationScope<T>
	{
		// public ConfigurationScope() : this( Items<ITransformer<T>>.Default ) {}
		public ConfigurationScope( params ITransformer<T>[] configurators ) : base( new ConfigurationSource<T>( configurators ).Global() ) {}
	}
}