using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup.Registration;
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
		readonly static Func<Type, Type> Parameters = ParameterTypes.Instance.ToDelegate();

		readonly Func<LocateTypeRequest, Type> locator = FactoryTypes.Instance.Get().ToDelegate();
		readonly Func<CompositionContract, CompositionContract> resolver;
		readonly Func<Activator.Parameter, object> delegateSource;

		protected FactoryExportDescriptorProviderBase( Func<Activator.Parameter, object> delegateSource ) : this( SelfTransformer<CompositionContract>.Instance.Get, delegateSource ) {}

		protected FactoryExportDescriptorProviderBase( Func<CompositionContract, CompositionContract> resolver, Func<Activator.Parameter, object> delegateSource )
		{
			this.resolver = resolver;
			this.delegateSource = delegateSource;
		}

		IDictionary<Type, CompositeActivator> CreateRegistry( DependencyAccessor accessor, params CompositionContract[] contracts )
		{
			var result = new Dictionary<Type, CompositeActivator>();
			foreach ( var contract in contracts )
			{
				CompositionDependency dependency;
				if ( accessor.TryResolveOptionalDependency( $"Activator Request for '{GetType().FullName}'", contract, true, out dependency ) )
				{
					result.Add( contract.ContractType, dependency.Target.GetDescriptor().Activator );
				}
			}
			return result;
		}

		sealed class Container : Cache<LifetimeContext, object>
		{
			readonly CompositionDependency dependency;
			readonly CompositeActivator activator, create, @new;

			public Container( CompositionDependency dependency, CompositeActivator activator )
			{
				this.dependency = dependency;
				this.activator = activator;

				create = Create;
				@new = New;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( create, NoMetadata );

			object Create( LifetimeContext context, CompositionOperation operation ) => dependency.Target.IsShared ? FromCache( context, operation ) : @new( context, operation );

			object FromCache( LifetimeContext context, CompositionOperation operation )
			{
				var key = context.FindContextWithin( Contracts.Default.Get( dependency.Contract ) );
				var result = Get( key ) ?? this.SetValue( key, @new( key, operation ) );
				return result;
			}

			object New( LifetimeContext context, CompositionOperation operation ) => context.Registered( activator( context, operation ) );
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			var exists = descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency );
			if ( !exists )
			{
				var resolved = resolver( contract );
				var type = resolved
					.With( compositionContract => new LocateTypeRequest( compositionContract.ContractType, compositionContract.ContractName ) )
					.With( locator );
				var success = type != null && descriptorAccessor.TryResolveOptionalDependency( "Category Request", contract.ChangeType( type ), true, out dependency );
				if ( success )
				{
					var registry = CreateRegistry( descriptorAccessor, new[] { resolved.ContractType, Parameters( resolved.ContractType ) }.WhereAssigned().Select( resolved.ChangeType ).ToArray() );
					
					CompositeActivator activator = new Activator( delegateSource, registry, resolved.ContractType ).Create;
					Func<IEnumerable<CompositionDependency>, ExportDescriptor> factory = new Container( dependency, activator ).Create;
					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, dependency.Target.IsShared, NoDependencies, factory );
				}
			}
		}
	}

	public class Activator
	{
		readonly Func<Parameter, object> factory;
		readonly Type factoryType;
		readonly Func<Type, CompositeActivator> get;

		public Activator( Func<Parameter, object> factory, IDictionary<Type, CompositeActivator> registry, Type factoryType )
		{
			this.factory = factory;
			this.factoryType = factoryType;
			get = registry.TryGet;
		}

		public class Parameter
		{
			readonly LifetimeContext context;
			readonly CompositionOperation operation;
			readonly Func<Type, CompositeActivator> factory;

			public Parameter( LifetimeContext context, CompositionOperation operation, Func<Type, CompositeActivator> factory, Type factoryType )
			{
				this.context = context;
				this.operation = operation;
				this.factory = factory;
				FactoryType = factoryType;
			}

			public T Activate<T>( Type type ) => (T)factory( type )?.Invoke( context, operation );

			public Type FactoryType { get; }
		}

		public object Create( LifetimeContext lifetime, CompositionOperation operation )
		{
			var context = new Parameter( lifetime, operation, get, factoryType );
			var result = factory( context );
			return result;
		}
	}

	public class ActivatorDelegateWithConversionFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorDelegateWithConversionFactory Instance { get; } = new ActivatorDelegateWithConversionFactory();
		ActivatorDelegateWithConversionFactory() {}

		public override Delegate Create( Activator.Parameter parameter ) => ActivatorDelegateFactory.Instance.Create( parameter ).Convert( ResultTypes.Instance.Get( parameter.FactoryType ) );
	}

	public class ActivatorDelegateFactory : FactoryBase<Activator.Parameter, Func<object>>
	{
		public static ActivatorDelegateFactory Instance { get; } = new ActivatorDelegateFactory();
		ActivatorDelegateFactory() {}

		public override Func<object> Create( Activator.Parameter parameter )
		{
			var factory = new FactoryDelegateLocatorFactory(
								new FactoryDelegateFactory( parameter.Activate<IFactory> ),
								new FactoryWithActivatedParameterDelegateFactory( new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create, parameter.Activate<object> ) 
								);

			var result = factory.Create( parameter.FactoryType );
			return result;
		}
	}

	public class ActivatorWithParameterDelegateFactory : FactoryBase<Activator.Parameter, Delegate>
	{
		public static ActivatorWithParameterDelegateFactory Instance { get; } = new ActivatorWithParameterDelegateFactory( ParameterTypes.Instance.ToDelegate(), ResultTypes.Instance.ToDelegate() );

		readonly Func<Type, Type> parameterLocator;
		readonly Func<Type, Type> resultLocator;

		ActivatorWithParameterDelegateFactory( Func<Type, Type> parameterLocator, Func<Type, Type> resultLocator )
		{
			this.parameterLocator = parameterLocator;
			this.resultLocator = resultLocator;
		}

		public override Delegate Create( Activator.Parameter parameter )
		{
			var @delegate = new FactoryWithParameterDelegateFactory( parameter.Activate<IFactoryWithParameter> ).Create( parameter.FactoryType );
			var result = @delegate.Convert( parameterLocator( parameter.FactoryType ), resultLocator( parameter.FactoryType ) );
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

		public override object Create( Activator.Parameter parameter ) => factory.Create( parameter ).Invoke();
	}

	public sealed class Contracts : FactoryCache<CompositionContract, string>
	{
		public static Contracts Default { get; } = new Contracts();
		Contracts() {}

		protected override string Create( CompositionContract parameter )
		{
			object sharingBoundaryMetadata = null;
			return parameter.MetadataConstraints?.ToDictionary( pair => pair.Key, pair => pair.Value ).TryGetValue( "SharingBoundary", out sharingBoundaryMetadata ) ?? false ? (string)sharingBoundaryMetadata : null;
		}
	}
}