using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public abstract class FactoryExportDescriptorProviderBase : ExportDescriptorProvider
	{
		readonly FactoryTypeRequestLocator locator;
		readonly ITransformer<CompositionContract> resolver;
		readonly ActivatorFactory factory;

		protected FactoryExportDescriptorProviderBase( [Required]FactoryTypeRequestLocator locator, [Required]ActivatorFactory factory ) : this( locator, SelfTransformer<CompositionContract>.Instance, factory ) {}

		protected FactoryExportDescriptorProviderBase( [Required]FactoryTypeRequestLocator locator, [Required]ITransformer<CompositionContract> resolver, [Required]ActivatorFactory factory )
		{
			this.locator = locator;
			this.resolver = resolver;
			this.factory = factory;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resultContract = resolver.Create( contract );
				var success = resultContract
					.With( compositionContract => new LocateTypeRequest( compositionContract.ContractType, compositionContract.ContractName ) )
					.With( locator.Create )
					.With( type => descriptorAccessor.TryResolveOptionalDependency( "Category Request", contract.ChangeType( type ), true, out dependency ) );
				if ( success )
				{
					var promise = dependency.Target;
					var boundary = ContractSupport.GetSharingBoundary( dependency.Contract );
					var activator = factory.Create( new ActivatorFactory.Parameter( descriptorAccessor, resultContract, dependency.Contract ) );
					
					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, promise.IsShared, NoDependencies,
						_ => ExportDescriptor.Create( ( context, operation ) =>
						{
							Func<object> create = () => activator( context, operation ).With( context.Checked ).With( o => new ActivationProperties.Factory( o ).Assign( promise.Contract.ContractType ) );
							var item = promise.IsShared ? new SharedValue( context.FindContextWithin( boundary ), resultContract, create ).Item : create();
							return item;
						}, NoMetadata ) );
				}
			}
		}

		class SharedValue : AssociatedValue<LifetimeContext, object>
		{
			public SharedValue( LifetimeContext instance, CompositionContract key, Func<object> create = null ) : base( instance, key.ToString(), create ) {}
		}
	}

	public class ActivatorFactory : FactoryBase<ActivatorFactory.Parameter, CompositeActivator>
	{
		public static ActivatorFactory Instance { get; } = new ActivatorFactory( ActivatorRegistryFactory.Instance, ActivatorResultFactory.Instance.Create );

		readonly ActivatorRegistryFactory registryFactory;
		readonly Func<Activator.Parameter, object> activatorFactory;

		public ActivatorFactory( [Required]ActivatorRegistryFactory registryFactory, Func<Activator.Parameter, object> activatorFactory )
		{
			this.registryFactory = registryFactory;
			this.activatorFactory = activatorFactory;
		}

		public class Parameter
		{
			public Parameter( DependencyAccessor accessor, CompositionContract resultContract, CompositionContract factoryContract )
			{
				Accessor = accessor;
				ResultContract = resultContract;
				FactoryContract = factoryContract;
			}

			public DependencyAccessor Accessor { get; }
			public CompositionContract ResultContract { get; }
			public CompositionContract FactoryContract { get; }
		}

		protected override CompositeActivator CreateItem( Parameter parameter )
		{
			var activators = registryFactory.Create( parameter );
			var activator = new Activator( activatorFactory, activators, parameter.FactoryContract.ContractType );
			var result = new CompositeActivator( activator.Create );
			return result;
		}

		public class ActivatorRegistry : Dictionary<Type, CompositeActivator>
		{
			readonly DependencyAccessor accessor;
			readonly CompositionContract contract;

			public ActivatorRegistry( [Required]DependencyAccessor accessor, [Required]CompositionContract contract )
			{
				this.accessor = accessor;
				this.contract = contract;
			}

			public void Register( [Required]Type type )
			{
				CompositionDependency dependency;
				if ( accessor.TryResolveOptionalDependency( $"Activator Request for '{GetType().FullName}'", contract.ChangeType( type ), true, out dependency ) )
				{
					Add( type, dependency.Target.GetDescriptor().Activator );
				}
			}
		}

		public class ActivatorRegistryFactory : FactoryBase<Parameter, ActivatorRegistry>
		{
			public static ActivatorRegistryFactory Instance { get; } = new ActivatorRegistryFactory();

			protected override ActivatorRegistry CreateItem( Parameter parameter )
			{
				var result = new ActivatorRegistry( parameter.Accessor, parameter.FactoryContract );
				new[] { parameter.FactoryContract.ContractType, Factory.GetParameterType( parameter.FactoryContract.ContractType ) }.NotNull().Each( result.Register );
				return result;
			}
		}
	}

	public class ActivatorDelegateFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorDelegateFactory Instance { get; } = new ActivatorDelegateFactory();

		protected override Delegate CreateItem( Activator.Parameter parameter )
		{
			var factory = new FactoryDelegateLocatorFactory(
								new FactoryDelegateFactory( parameter.Activate<IFactory> ),
								new FactoryWithActivatedParameterDelegateFactory( new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create, parameter.Activate<object> ) );

			var result = factory.Create( parameter.FactoryType ).Convert( Factory.GetResultType( parameter.FactoryType ) );
			return result;
		}
	}

	public class ActivatorWithParameterDelegateFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorWithParameterDelegateFactory Instance { get; } = new ActivatorWithParameterDelegateFactory();

		protected override Delegate CreateItem( Activator.Parameter parameter )
		{
			var @delegate = new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create( parameter.FactoryType );
			var result = @delegate.Convert( Factory.GetParameterType( parameter.FactoryType ), Factory.GetResultType( parameter.FactoryType ) );
			return result;
		}
	}

	public class ActivatorResultFactory : FactoryBase<Activator.Parameter, object>
	{
		public static ActivatorResultFactory Instance { get; } = new ActivatorResultFactory();

		readonly ActivatorDelegateFactory factory;

		public ActivatorResultFactory() : this( ActivatorDelegateFactory.Instance ) {}

		public ActivatorResultFactory( ActivatorDelegateFactory factory )
		{
			this.factory = factory;
		}

		protected override object CreateItem( Activator.Parameter parameter )
		{
			var @delegate = factory.Create( parameter );
			var result = @delegate.DynamicInvoke();
			return result;
		}
	}

	public class Activator
	{
		readonly Func<Parameter, object> factory;
		readonly IDictionary<Type, CompositeActivator> activators;
		readonly Type factoryType;

		public Activator( [Required]Func<Parameter, object> factory, [Required]IDictionary<Type, CompositeActivator> activators, Type factoryType )
		{
			this.factory = factory;
			this.activators = activators;
			this.factoryType = factoryType;
		}

		public class Parameter
		{
			readonly LifetimeContext context;
			readonly CompositionOperation operation;
			readonly Func<Type, CompositeActivator> factory;

			public Parameter( [Required]LifetimeContext context, [Required]CompositionOperation operation, [Required]Func<Type, CompositeActivator> factory, [Required]Type factoryType )
			{
				this.context = context;
				this.operation = operation;
				this.factory = factory;
				FactoryType = factoryType;
			}

			public T Activate<T>( [Required] Type type )
			{
				var compositeActivator = factory( type );
				return compositeActivator.With( activator => (T)activator( context, operation ) );
			}

			public Type FactoryType { get; }
		}

		public object Create( [Required]LifetimeContext lifetime, [Required]CompositionOperation operation )
		{
			var context = new Parameter( lifetime, operation, type => activators.TryGet( type ), factoryType );
			var result = factory( context );
			return result;
		}
	}

	public static class ContractSupport
	{
		[Freeze]
		public static string GetSharingBoundary( CompositionContract contract )
		{
			object sharingBoundaryMetadata;
			var result = contract.MetadataConstraints.With( pairs => pairs.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ? (string)sharingBoundaryMetadata : null );
			return result;
		}
	}
}