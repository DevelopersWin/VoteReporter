using DragonSpark.Sources;
using System.Collections.Immutable;

namespace DragonSpark.Composition
{
	public sealed class SingletonExportsSource : Scope<ImmutableArray<SingletonExport>>
	{
		public static ISource<ImmutableArray<SingletonExport>> Default { get; } = new SingletonExportsSource();
		SingletonExportsSource() : base( Factory.Global( () => ExportsProfileFactory.Default.Get().Singletons.ToImmutableArray() ) ) {}
	}
}