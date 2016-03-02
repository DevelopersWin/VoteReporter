using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Composition
{
	public abstract class FactoryExportDescriptorProviderBase : ExportDescriptorProvider
	{
		readonly DiscoverableFactoryTypeLocator locator;
		readonly Func<CompositionContract, CompositionContract> resolveContract;
		readonly Func<Type, Func<object>, object> delegateHandler;

		// protected FactoryExportDescriptorProviderBase( Assembly[] assemblies, [Required]Func<CompositionContract, CompositionContract> resolveContract, [Required]Func<Func<object>, object> delegateHandler ) : this( new DiscoverableFactoryTypeLocator( assemblies ), resolveContract, delegateHandler ) {}

		protected FactoryExportDescriptorProviderBase( 
			[Required]DiscoverableFactoryTypeLocator locator,
			[Required]Func<CompositionContract, CompositionContract> resolveContract, 
			[Required]Func<Type, Func<object>, object> delegateHandler
			)
		{
			this.locator = locator;
			this.resolveContract = resolveContract;
			this.delegateHandler = delegateHandler;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				var resolvedContract = resolveContract( contract ).With( compositionContract => compositionContract.ContractType );
				var factory = resolvedContract.With( locator.Create );
				if ( factory != null && descriptorAccessor.TryResolveOptionalDependency( "Factory Request", contract.ChangeType( factory ), true, out dependency ) )
				{
					var promise = dependency.Target;
					var descriptor = promise.GetDescriptor();

					yield return new ExportDescriptorPromise( contract, promise.Origin, promise.IsShared, () => promise.Dependencies,
						dependencies => ExportDescriptor.Create( ( context, operation ) => delegateHandler( resolvedContract, descriptor.Activator( context, operation ).To<IFactory>().Create ), descriptor.Metadata )
						);
				}
			}
		}
	}
}