using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
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

		readonly ICache<LifetimeContext, object> cache = new Cache<LifetimeContext, object>();
		readonly Func<LocateTypeRequest, Type> locator = SourceTypes.Instance.Get().ToDelegate();
		readonly Func<ActivatorParameter, ISource> activatorSource;
		readonly Func<CompositionContract, CompositionContract> resolver;

		protected FactoryExportDescriptorProviderBase( Func<ActivatorParameter, ISource> activatorSource ) : this( activatorSource, SelfTransformer<CompositionContract>.Instance.Get ) {}

		protected FactoryExportDescriptorProviderBase( Func<ActivatorParameter, ISource> activatorSource, Func<CompositionContract, CompositionContract> resolver )
		{
			this.resolver = resolver;
			this.activatorSource = activatorSource;
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

					var source = activatorSource( new ActivatorParameter( registry, resolved.ContractType ) );

					var context = new Container( source );
					var factory = dependency.Target.IsShared ? new SharedFactory( context.Create, cache, Contracts.Default.Get( dependency.Contract ) ) : new Factory( context.Create );
					yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, dependency.Target.IsShared, NoDependencies, factory.Create );
				}
			}
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

		class Factory : FactoryBase<IEnumerable<CompositionDependency>, ExportDescriptor>
		{
			readonly CompositeActivator activator, create;

			public Factory( CompositeActivator activator )
			{
				this.activator = activator;
				create = Create;
			}

			public override ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( create, NoMetadata );

			protected virtual object Create( LifetimeContext context, CompositionOperation operation ) => context.Registered( activator( context, operation ) );
		}

		class SharedFactory : Factory
		{
			readonly ICache<LifetimeContext, object> cache;
			readonly string boundary;

			public SharedFactory( CompositeActivator activator, ICache<LifetimeContext, object> cache, string boundary ) : base( activator )
			{
				this.cache = cache;
				this.boundary = boundary;
			}

			protected override object Create( LifetimeContext context, CompositionOperation operation )
			{
				var key = context.FindContextWithin( boundary );
				var result = cache.Get( key ) ?? cache.SetValue( key, base.Create( key, operation ) );
				return result;
			}
		}
	}

	class Container
	{
		public static IStackSource<CompositeActivatorParameters> Stack { get; } = new AmbientStack<CompositeActivatorParameters>();

		readonly ISource source;

		public Container( ISource source )
		{
			this.source = source;
		}

		public object Create( LifetimeContext context, CompositionOperation operation )
		{
			using ( Stack.Assignment( new CompositeActivatorParameters( context, operation ) ) )
			{
				return source.Get();
			}
		}
	}

	public class ActivatorDelegateFactory : FactoryBase<ActivatorParameter, ISource<Func<object>>>
	{
		public static ActivatorDelegateFactory Instance { get; } = new ActivatorDelegateFactory();
		ActivatorDelegateFactory() {}

		public override ISource<Func<object>> Create( ActivatorParameter parameter )
		{
			var provider = new CompositeActivatorServiceProvider( parameter.Registry );
			var result = new FixedFactory<Type, Func<object>>( new SourceDelegates( provider.Self ).Get, parameter.FactoryType );
			return result;
		}

		/*class SourceFactory : FixedFactory<Type, Func<object>>
		{
			readonly Func<Type, CompositeActivator> source;
			readonly IStackSource<CompositeActivatorParameters> stack;

			public SourceFactory( Func<Type, CompositeActivator> source, Func<Type, Func<object>> factory, Type parameter  ) : this( source, Container.Stack, factory, parameter ) {}

			SourceFactory( Func<Type, CompositeActivator> source, IStackSource<CompositeActivatorParameters> stack, Func<Type, Func<object>> factory, Type parameter  ) : base( factory, parameter )
			{
				this.source = source;
				this.stack = stack;
			}

			public object GetService( Type serviceType )
			{
				var current = stack.GetCurrentItem();
				var result = source( serviceType )?.Invoke( current.Context, current.Operation );
				return result;
			}
		}*/
	}

	class CompositeActivatorServiceProvider : IServiceProvider
	{
		readonly Func<Type, CompositeActivator> source;
		readonly IStackSource<CompositeActivatorParameters> stack;

		public CompositeActivatorServiceProvider( Func<Type, CompositeActivator> source ) : this( source, Container.Stack ) {}

		CompositeActivatorServiceProvider( Func<Type, CompositeActivator> source, IStackSource<CompositeActivatorParameters> stack )
		{
			this.source = source;
			this.stack = stack;
		}

		public object GetService( Type serviceType )
		{
			var current = stack.GetCurrentItem();
			var result = source( serviceType )?.Invoke( current.Context, current.Operation );
			return result;
		}
	}

	public struct CompositeActivatorParameters
	{
		public CompositeActivatorParameters( LifetimeContext context, CompositionOperation operation )
		{
			Context = context;
			Operation = operation;
		}

		public LifetimeContext Context { get; }
		public CompositionOperation Operation { get; }
	}

	public struct ActivatorParameter
	{
		public ActivatorParameter( IDictionary<Type, CompositeActivator> registry, Type factoryType ) : this( registry.TryGet, factoryType ) {}

		ActivatorParameter( Func<Type, CompositeActivator> registry, Type factoryType )
		{
			Registry = registry;
			FactoryType = factoryType;
		}

		public Func<Type, CompositeActivator> Registry { get; }
		public Type FactoryType { get; }
	}

	/*public class Activator
	{
		readonly Func<Parameter, object> factory;
		readonly Type factoryType;
		readonly Func<Type, CompositeActivator> get;

		public Activator( Func<Parameter, object> factory, IDictionary<Type, CompositeActivator> registry, Type factoryType ) : this( factory, registry.TryGet, factoryType ) {}

		public Activator( Func<Parameter, object> factory, Func<Type, CompositeActivator> registry, Type factoryType )
		{
			this.factory = factory;
			this.factoryType = factoryType;
			get = registry;
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
	}*/

	public class ActivatorDelegateWithConversionFactory : FactoryBase<ActivatorParameter, ISource>
	{
		public static ActivatorDelegateWithConversionFactory Instance { get; } = new ActivatorDelegateWithConversionFactory();
		ActivatorDelegateWithConversionFactory() {}

		public override ISource Create( ActivatorParameter parameter ) => ActivatorDelegateFactory.Instance.Create( parameter ).Get().Convert( ResultTypes.Instance.Get( parameter.FactoryType ) ).Sourced();
	}

	public class ActivatorWithParameterDelegateFactory : FactoryBase<ActivatorParameter, ISource>
	{
		public static ActivatorWithParameterDelegateFactory Instance { get; } = new ActivatorWithParameterDelegateFactory();
		ActivatorWithParameterDelegateFactory() : this( ParameterTypes.Instance.ToDelegate(), ResultTypes.Instance.ToDelegate() ) {}

		readonly Func<Type, Type> parameterLocator;
		readonly Func<Type, Type> resultLocator;

		ActivatorWithParameterDelegateFactory( Func<Type, Type> parameterLocator, Func<Type, Type> resultLocator )
		{
			this.parameterLocator = parameterLocator;
			this.resultLocator = resultLocator;
		}

		public override ISource Create( ActivatorParameter parameter )
		{
			var provider = new CompositeActivatorServiceProvider( parameter.Registry );
			var @delegate = new ParameterizedSourceDelegates( provider.Self ).Get( parameter.FactoryType );
			var result = @delegate.Convert( parameterLocator( parameter.FactoryType ), resultLocator( parameter.FactoryType ) ).Sourced();
			return result;
		}
	}

	/*public class ActivatorResultFactory : FactoryBase<Activator.Parameter, object>
	{
		public static ActivatorResultFactory Instance { get; } = new ActivatorResultFactory();

		readonly ActivatorDelegateFactory factory;

		public ActivatorResultFactory() : this( ActivatorDelegateFactory.Instance ) {}

		public ActivatorResultFactory( ActivatorDelegateFactory factory )
		{
			this.factory = factory;
		}

		public override object Create( Activator.Parameter parameter ) => factory.Create( parameter ).Invoke();
	}*/

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