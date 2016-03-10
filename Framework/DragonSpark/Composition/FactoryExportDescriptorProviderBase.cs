using System;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using Handler = System.Func<System.Type, System.Func<object>, object>;
using TransformContract = System.Func<System.Composition.Hosting.Core.CompositionContract, System.Composition.Hosting.Core.CompositionContract>;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public class AccessMonitor<T>
	{
		readonly IWritableValue<bool> enabled = new FixedValue<bool>().With( value => value.Assign( true ) );

		public AccessMonitor() : this( default(T) ) {}

		public AccessMonitor( T defaultValue )
		{
			DefaultValue = defaultValue;
		}

		public T DefaultValue { get; }

		public T Access( Func<T> result )
		{
			if ( enabled.Item )
			{
				using ( new AssignValueCommand<bool>( enabled, true ).ExecuteWith( false ) )
				{
					return result();
				}
			}
			return DefaultValue;
		}
	}

	public abstract class FactoryExportDescriptorProviderBase : ExportDescriptorProvider
	{
		readonly DiscoverableFactoryTypeLocator locator;
		readonly TransformContract resolveContract;
		readonly Handler delegateHandler;
		readonly AccessMonitor<IEnumerable<ExportDescriptorPromise>> monitor = new AccessMonitor<IEnumerable<ExportDescriptorPromise>>( Default<ExportDescriptorPromise>.Items );
		
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

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )//  => monitor.Access( () =>
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resolvedContract = resolveContract( contract ).With( c => c.ContractType );
				var success = resolvedContract.With( locator.Create ).With( type => descriptorAccessor.TryResolveOptionalDependency( "Factory Request", contract.ChangeType( type ), true, out dependency ) );
				if ( success )
				{
					var promise = dependency.Target;
					var descriptor = promise.GetDescriptor();
					var boundary = GetSharingBoundary( dependency.Contract );
					return new ExportDescriptorPromise( contract, promise.Origin, promise.IsShared, () => Default<CompositionDependency>.Items,
						_ => ExportDescriptor.Create( ( context, operation ) =>
						{
							Func<object> activator = () => delegateHandler( resolvedContract, descriptor.Activator( context, operation ).To<IFactory>().Create );
							var item = promise.IsShared ? new SharedValue( context.FindContextWithin( boundary ), resolvedContract, activator ).Item : activator();
							return item;
						}, descriptor.Metadata )
						).ToItem();;
				}
			}
			return Default<ExportDescriptorPromise>.Items;
		}// );

		class SharedValue : AssociatedValue<LifetimeContext, object>
		{
			public SharedValue( LifetimeContext instance, Type key, Func<object> create = null ) : base( instance, key, create ) {}
		}
	}
}