using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Diagnostics;
using System.Linq;
using DragonSpark.Activation;
using DragonSpark.Aspects;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;
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

		class Activator
		{
			readonly CompositionContract contract;
			readonly DependencyAccessor accessor;
			readonly IDictionary<Type, CompositeActivator> activators = new Dictionary<Type, CompositeActivator>();

			public Activator( [Required]CompositionContract contract, [Required]DependencyAccessor accessor, params Type[] types )
			{
				this.contract = contract;
				this.accessor = accessor;

				types.Each( Register );
			}

			void Register( [Required]Type type )
			{
				CompositionDependency dependency;
				if ( accessor.TryResolveOptionalDependency( $"Activator Request for '{GetType().FullName}'", contract.ChangeType( type ), true, out dependency ) )
				{
					activators.Add( type, dependency.Target.GetDescriptor().Activator );
				}
				else
				{
					Debugger.Break();
				}
			}

			class Context
			{
				readonly LifetimeContext context;
				readonly CompositionOperation operation;
				readonly Func<Type, CompositeActivator> factory;

				public Context( [Required]LifetimeContext context, [Required]CompositionOperation operation, [Required]Func<Type, CompositeActivator> factory )
				{
					this.context = context;
					this.operation = operation;
					this.factory = factory;
				}

				public T Activate<T>( [Required] Type type ) => factory( type ).With( activator => (T)activator( context, operation ) );
			}

			public Func<object> Create( [Required]LifetimeContext lifetime, [Required]CompositionOperation operation )
			{
				var context = new Context( lifetime, operation, type => activators.TryGet( type ) );

				var factory = new FactoryDelegateLocatorFactory(
								new FactoryDelegateFactory( context.Activate<IFactory> ),
								new FactoryWithActivatedParameterDelegateFactory( new FactoryWithParameterDelegateFactory( context.Activate<IFactoryWithParameter> ).Create, context.Activate<object> ) );
				var result = factory.Create( contract.ContractType );
				return result;
			}
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resolved = resolveContract( contract );
				var with = resolved.With( locator.Create );
				var success = with.With( type => descriptorAccessor.TryResolveOptionalDependency( "Factory Request", contract.ChangeType( type ), true, out dependency ) );
				if ( success )
				{
					var promise = dependency.Target;
					var boundary = ContractSupport.GetSharingBoundary( dependency.Contract );
					var activator = new Activator( dependency.Contract, descriptorAccessor, dependency.Contract.ContractType.Append( Factory.GetParameterType( dependency.Contract.ContractType ) ).NotNull().ToArray() );

					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, promise.IsShared, NoDependencies,
						_ => ExportDescriptor.Create( ( context, operation ) =>
						{
							Func<object> create = () => delegateHandler( resolved.ContractType, activator.Create( context, operation ) ).With( o => new ExportProperties.Factory( o ).Assign( promise ) );
							var item = promise.IsShared ? new SharedValue( context.FindContextWithin( boundary ), resolved.ContractType, create ).Item : create();
							return item;
						}, NoMetadata ) );
				}
			}
		}

		class SharedValue : AssociatedValue<LifetimeContext, object>
		{
			public SharedValue( LifetimeContext instance, Type key, Func<object> create = null ) : base( instance, key, create ) {}
		}
	}

	public static class ContractSupport
	{
		// [Freeze]
		public static string GetSharingBoundary( CompositionContract contract )
		{
			object sharingBoundaryMetadata;
			var result = contract.MetadataConstraints.With( pairs => pairs.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ? (string)sharingBoundaryMetadata : null );
			return result;
		}
	}

	public static class ExportProperties
	{
		public class Instance : AssociatedValue<bool>
		{
			public Instance( object instance ) : base( instance ) {}
		}

		public class Factory : AssociatedValue<ExportDescriptorPromise>
		{
			public Factory( object instance ) : base( instance ) {}
		}
	}
}