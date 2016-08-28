using DragonSpark.Composition;
using DragonSpark.Sources;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.TypeSystem;

namespace DragonSpark.Application
{
	sealed class DefaultExportProvider : IExportProvider
	{
		public static DefaultExportProvider Default { get; } = new DefaultExportProvider();
		DefaultExportProvider() {}

		public ImmutableArray<T> GetExports<T>( string name = null ) => ApplicationParts.IsAssigned ? Exports<T>.DefaultNested.Get() : Items<T>.Immutable;

		sealed class Exports<T> : ItemSourceBase<T>
		{
			public static Exports<T> DefaultNested { get; } = new Exports<T>();
			Exports() : this( TypeAssignableSpecification<T>.Default.ToSpecificationDelegate() ) {}

			readonly Func<Type, bool> specification;

			Exports( Func<Type, bool> specification )
			{
				this.specification = specification;
			}

			protected override IEnumerable<T> Yield()
			{
				var singletonExports = SingletonExportsSource.Default.Get();
				foreach ( var export in singletonExports )
				{
					if ( export.Contracts.Select( contract => contract.ContractType ).Any( specification ) )
					{
						var item = export.Factory();
						if ( item is T )
						{
							yield return (T)item;
						}
					}
				}
			}
		}
	}
}