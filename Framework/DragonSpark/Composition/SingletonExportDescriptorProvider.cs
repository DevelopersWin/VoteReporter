using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting.Core;
using System.Linq;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;

namespace DragonSpark.Composition
{
	public sealed class SingletonExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly ImmutableArray<SingletonExport> singletons;
		readonly static Func<Type, SingletonExport> Selector = SingletonExports.Instance.Get;

		public SingletonExportDescriptorProvider( params Type[] types ) : this ( types.Select( Selector ).WhereAssigned().ToImmutableArray() ) {}

		public SingletonExportDescriptorProvider( ImmutableArray<SingletonExport> singletons )
		{
			this.singletons = singletons;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			foreach ( var singleton in singletons )
			{
				if ( singleton.Contracts.Contains( contract ) )
				{
					yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Factory( singleton.Factory ).Create );
				}
			}
		}

		sealed class Factory : DelegatedSource<object>
		{
			readonly CompositeActivator activate;

			public Factory( Func<object> provider ) : base( provider )
			{
				activate = Activate;
			}

			object Activate( LifetimeContext context, CompositionOperation operation ) => Get();

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( activate, NoMetadata );
		}
	}
}