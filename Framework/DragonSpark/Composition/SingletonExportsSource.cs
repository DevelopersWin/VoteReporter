using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Composition
{
	public sealed class SingletonExportsSource : Scope<ImmutableArray<SingletonExport>>
	{
		readonly static Func<Type, SingletonExport> Selector = SingletonExports.Default.Get;

		public static ISource<ImmutableArray<SingletonExport>> Default { get; } = new SingletonExportsSource();
		SingletonExportsSource() : base( Factory.Global( () => ApplicationTypes.Default.Get().Select( Selector ).WhereAssigned().ToImmutableArray() ) ) {}
	}
}