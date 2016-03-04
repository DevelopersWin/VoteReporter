using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using Handler = System.Func<System.Type, System.Func<object>, object>;
using TransformContract = System.Func<System.Composition.Hosting.Core.CompositionContract, System.Composition.Hosting.Core.CompositionContract>;

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

		static object FromContext( LifetimeContext context, string boundary, int key, CompositionOperation operation, CompositeActivator activator )
		{
			var scope = context.FindContextWithin( boundary );
			var scoped = ReferenceEquals( scope, context );
			var result = scoped ? scope.GetOrCreate( key, operation, activator ) : CompositionOperation.Run(scope, (c1, o1) => c1.GetOrCreate( key, o1, activator ) );
			return result;
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
					var key = LifetimeContext.AllocateSharingId();
					var boundary = GetSharingBoundary( dependency.Contract );
					yield return new ExportDescriptorPromise( contract, promise.Origin, promise.IsShared, () => promise.Dependencies,
						dependencies => ExportDescriptor.Create( ( context, operation ) =>
						{
							CompositeActivator activator = ( c, o ) => delegateHandler( resolvedContract, descriptor.Activator( c, o ).To<IFactory>().Create );
							var item = promise.IsShared ? FromContext( context, boundary, key, operation, activator ) : activator( context, operation );
							return item;
						}, descriptor.Metadata )
					);
				}
			}
		}
	}
}