using System;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using Handler = System.Func<System.Type, System.Func<object>, object>;
using TransformContract = System.Func<System.Composition.Hosting.Core.CompositionContract, System.Composition.Hosting.Core.CompositionContract>;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public abstract class FactoryExportDescriptorProviderBase : ExportDescriptorProvider
	{
		readonly DiscoverableFactoryTypeLocator locator;
		readonly TransformContract resolveContract;
		readonly Handler delegateHandler;

		protected FactoryExportDescriptorProviderBase( [Required]DiscoverableFactoryTypeLocator locator, [Required]TransformContract resolveContract, [Required]Handler delegateHandler )
		{
			this.locator = locator;
			this.resolveContract = resolveContract;
			this.delegateHandler = delegateHandler;
		}

		static string GetSharingBoundary( CompositionContract contract )
		{
			object sharingBoundaryMetadata;
			var result = contract.MetadataConstraints.With( pairs => pairs.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ? (string)sharingBoundaryMetadata : null );
			return result;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				var resolvedContract = resolveContract( contract ).With( c => c.ContractType );
				var factory = resolvedContract.With( locator.Create );
				if ( factory != null && descriptorAccessor.TryResolveOptionalDependency( "Factory Request", contract.ChangeType( factory ), true, out dependency ) )
				{
					var promise = dependency.Target;
					var descriptor = promise.GetDescriptor();
					var boundary = GetSharingBoundary( dependency.Contract );
					yield return new ExportDescriptorPromise( contract, promise.Origin, promise.IsShared, () => Default<CompositionDependency>.Items,
						_ => ExportDescriptor.Create( ( context, operation ) =>
						{
							Func<object> activator = () => delegateHandler( resolvedContract, descriptor.Activator( context, operation ).To<IFactory>().Create );
							var item = promise.IsShared ? new SharedValue( context.FindContextWithin( boundary ), resolvedContract, activator ).Item : activator();
							return item;
						}, descriptor.Metadata )
					);
				}
			}
		}

		class SharedValue : AssociatedValue<LifetimeContext, object>
		{
			public SharedValue( LifetimeContext instance, Type key, Func<object> create = null ) : base( instance, key, create ) {}
		}
	}
}