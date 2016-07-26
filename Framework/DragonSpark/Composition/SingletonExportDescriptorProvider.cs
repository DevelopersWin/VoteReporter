using DragonSpark.Activation;
using DragonSpark.Extensions;
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

		public SingletonExportDescriptorProvider( params Type[] types ) : this ( types.Select( SingletonExports.Instance.Get ).WhereAssigned().ToImmutableArray() ) {}

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

		sealed class Factory : DelegatedFactory<object>
		{
			readonly CompositeActivator activate;

			public Factory( Func<object> provider ) : base( provider )
			{
				activate = Activate;
			}

			object Activate( LifetimeContext context, CompositionOperation operation ) => Create();

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( activate, NoMetadata );
		}
	}
}