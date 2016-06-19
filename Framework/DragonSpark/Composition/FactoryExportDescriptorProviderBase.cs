using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public abstract class FactoryExportDescriptorProviderBase : ExportDescriptorProvider
	{
		readonly Func<LocateTypeRequest, Type> locator;
		readonly ITransformer<CompositionContract> resolver;
		readonly Func<Activator.Parameter, object> delegateSource;
		
		protected FactoryExportDescriptorProviderBase( FactoryTypeLocator locator, Func<Activator.Parameter, object> delegateSource ) : this( locator, SelfTransformer<CompositionContract>.Instance, delegateSource ) {}

		protected FactoryExportDescriptorProviderBase( FactoryTypeLocator locator, ITransformer<CompositionContract> resolver, Func<Activator.Parameter, object> delegateSource ) : this( locator.ToDelegate(), resolver, delegateSource ) {}

		FactoryExportDescriptorProviderBase( Func<LocateTypeRequest, Type> locator, ITransformer<CompositionContract> resolver, Func<Activator.Parameter, object> delegateSource )
		{
			this.locator = locator;
			this.resolver = resolver;
			this.delegateSource = delegateSource;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resultContract = resolver.Create( contract );
				var type = resultContract
					.With( compositionContract => new LocateTypeRequest( compositionContract.ContractType, compositionContract.ContractName ) )
					.With( locator );
				var success = type != null && descriptorAccessor.TryResolveOptionalDependency( "Category Request", contract.ChangeType( type ), true, out dependency );
				if ( success )
				{
					yield return new Promise( dependency, GetType().Name, resultContract, descriptorAccessor, delegateSource );
				}
			}
		}

		class Promise : ExportDescriptorPromise
		{
			public Promise( CompositionDependency dependency, string origin, CompositionContract resultContract, DependencyAccessor descriptorAccessor, Func<Activator.Parameter, object> delegateSource ) : this( dependency, origin, new ActivatorFactory( ActivatorFactory.ActivatorRegistryFactory.Instance, delegateSource ).Create( new ActivatorFactory.Parameter( descriptorAccessor, resultContract, dependency.Contract ) ) ) {}
			Promise( CompositionDependency dependency, string origin, CompositeActivator activator ) : this( dependency, origin, new Context( dependency, activator ) ) {}
			Promise( CompositionDependency dependency, string origin, Context context ) : base( dependency.Contract, origin, dependency.Target.IsShared, NoDependencies, context.Create ) {}

			class Context : FactoryBase<Context.Parameter, object>
			{
				readonly CacheContext<LifetimeContext, Parameter, object> cache;

				readonly CompositionDependency dependency;
				readonly CompositeActivator activator;
				readonly CompositeActivator create;
				readonly Func<Parameter, object> @new;

				public Context( CompositionDependency dependency, CompositeActivator activator )
				{
					this.dependency = dependency;
					this.activator = activator;

					cache = new CacheContext<LifetimeContext, Parameter, object>( Factory );
					create = Create;
					@new = New;
				}

				public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( create, NoMetadata );

				object Create( LifetimeContext context, CompositionOperation operation ) => Create( new Parameter( context, operation ) );

				public override object Create( Parameter parameter ) => dependency.Target.IsShared ? FromCache( parameter ) : @new( parameter );

				object FromCache( Parameter parameter )
				{
					var boundary = ContractSupport.Default.Get( dependency.Contract );
					var context = parameter.Context.FindContextWithin( boundary );
					var result = cache.GetOrSet( context, parameter );
					return result;
				}

				Func<LifetimeContext, object> Factory( Parameter parameter ) => new FixedFactory<Parameter, object>( @new, parameter ).Wrap<object>().ToDelegate();

				object New( Parameter parameter )
				{
					var result = activator( parameter.Context, parameter.Operation );
					if ( result != null )
					{
						parameter.Context.Checked( result );
						ActivationProperties.Factory.Set( result, dependency.Target.Contract.ContractType );
					}
					return result;
				}

				public struct Parameter
				{
					public Parameter( LifetimeContext context, CompositionOperation operation )
					{
						Context = context;
						Operation = operation;
					}

					public LifetimeContext Context { get; }
					public CompositionOperation Operation { get; }
				}
			}
		}
	}

	public class ActivatorFactory : FactoryBase<ActivatorFactory.Parameter, CompositeActivator>
	{
		readonly ActivatorRegistryFactory registryFactory;
		readonly Func<Activator.Parameter, object> activatorFactory;

		public ActivatorFactory( [Required]ActivatorRegistryFactory registryFactory, Func<Activator.Parameter, object> activatorFactory )
		{
			this.registryFactory = registryFactory;
			this.activatorFactory = activatorFactory;
		}

		public struct Parameter
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

		public override CompositeActivator Create( Parameter parameter )
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

			public override ActivatorRegistry Create( Parameter parameter )
			{
				var result = new ActivatorRegistry( parameter.Accessor, parameter.FactoryContract );
				new[] { parameter.FactoryContract.ContractType, ParameterTypeLocator.Instance.Get( parameter.FactoryContract.ContractType ) }.WhereAssigned().Each( result.Register );
				return result;
			}
		}
	}

	public class ActivatorDelegateWithConversionFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorDelegateWithConversionFactory Instance { get; } = new ActivatorDelegateWithConversionFactory();

		public override Delegate Create( Activator.Parameter parameter ) => 
			ActivatorDelegateFactory.Instance.Create( parameter ).Convert( ResultTypeLocator.Instance.Get( parameter.FactoryType ) );
	}

	public class ActivatorDelegateFactory : FactoryBase<Activator.Parameter, Func<object>>
	{
		public static ActivatorDelegateFactory Instance { get; } = new ActivatorDelegateFactory();

		public override Func<object> Create( Activator.Parameter parameter )
		{
			var factory = new FactoryDelegateLocatorFactory(
								new FactoryDelegateFactory( parameter.Activate<IFactory> ),
								new FactoryWithActivatedParameterDelegateFactory( new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create, parameter.Activate<object> ) );

			var result = factory.Create( parameter.FactoryType );
			return result;
		}
	}

	public class ActivatorWithParameterDelegateFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorWithParameterDelegateFactory Instance { get; } = new ActivatorWithParameterDelegateFactory();

		public override Delegate Create( Activator.Parameter parameter )
		{
			var @delegate = new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create( parameter.FactoryType );
			var result = @delegate.Convert( ParameterTypeLocator.Instance.Get( parameter.FactoryType ), ResultTypeLocator.Instance.Get( parameter.FactoryType ) );
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

		public override object Create( Activator.Parameter parameter )
		{
			var create = factory.Create( parameter );
			var result = create();
			return result;
		}
	}

	public class Activator
	{
		readonly Func<Parameter, object> factory;
		readonly Type factoryType;
		readonly Func<Type, CompositeActivator> get;

		public Activator( [Required]Func<Parameter, object> factory, [Required]IDictionary<Type, CompositeActivator> activators, Type factoryType )
		{
			this.factory = factory;
			this.factoryType = factoryType;
			get = activators.TryGet;
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
				var result = (T)compositeActivator?.Invoke( context, operation );
				return result;
			}

			public Type FactoryType { get; }
		}

		public object Create( [Required]LifetimeContext lifetime, [Required]CompositionOperation operation )
		{
			var context = new Parameter( lifetime, operation, get, factoryType );
			var result = factory( context );
			return result;
		}
	}

	public class ContractSupport : Cache<CompositionContract, string>
	{
		public static ContractSupport Default { get; } = new ContractSupport();

		ContractSupport() : base( GetSharingBoundary ) {}

		static string GetSharingBoundary( CompositionContract contract )
		{
			object sharingBoundaryMetadata;
			var result = contract.MetadataConstraints != null ? ( contract.MetadataConstraints.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ? (string)sharingBoundaryMetadata : null ) : null;
			return result;
		}
	}
}