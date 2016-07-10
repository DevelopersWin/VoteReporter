using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Setup
{
	public class AssemblyBasedServiceProviderFactory : ServiceProviderFactory
	{
		public AssemblyBasedServiceProviderFactory( IEnumerable<Assembly> assemblies ) : base( new Composition.AssemblyBasedServiceProviderFactory( assemblies ).Create ) {}
	}

	public class TypeBasedServiceProviderFactory : ServiceProviderFactory
	{
		public TypeBasedServiceProviderFactory( IEnumerable<Type> types ) : base( new Composition.TypeBasedServiceProviderFactory( types ).Create ) {}
	}

	public class ServiceProviderFactory : ConfiguredServiceProviderFactory<ConfigureProviderCommand>
	{
		/*public ServiceProviderFactory( Type[] types ) : this( new TypeBasedServiceProviderFactory( types ) ) {}

		public ServiceProviderFactory( Assembly[] assemblies ) : this( new AssemblyBasedServiceProviderFactory( assemblies ) ) {}*/

		// public ServiceProviderFactory( IFactory<ContainerConfiguration> source ) : this( new Composition.ServiceProviderFactory( source ) ) {}

		public ServiceProviderFactory( Func<IServiceProvider> provider ) : base( provider ) {}
	}

	public sealed class ConfigureProviderCommand : CommandBase<IServiceProvider>
	{
		readonly ILogger logger;
		readonly IServiceProviderHost host;

		public ConfigureProviderCommand( [Required]ILogger logger, [Required]IServiceProviderHost host )
		{
			this.logger = logger;
			this.host = host;
		}

		public override void Execute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton, Items<object>.Default );

			var assign = new AssignValueCommand<IServiceProvider>( host ).AsExecuted( parameter );
			parameter.Get<IDisposableRepository>()?.Add( assign );
		}
	}

	public class AssignServiceProvider : AssignValueCommand<IServiceProvider>
	{
		public AssignServiceProvider( IServiceProvider current ) : this( CurrentServiceProvider.Instance, current ) {}

		public AssignServiceProvider( IWritableStore<IServiceProvider> store, IServiceProvider current ) : base( store, current ) {}
	}

	public static class ApplicationExtensions
	{
		public static IApplication<T> AsExecuted<T>( this IApplication<T> @this, T arguments )
		{
			using ( var command = new ExecuteApplicationCommand<T>( @this ) )
			{
				command.Execute( arguments );
			}
			return @this;
		}
	}

	public class ExecuteApplicationCommand<T> : DisposingCommand<T>
	{
		readonly IApplication<T> application;
		readonly AssignServiceProvider assign;

		public ExecuteApplicationCommand( [Required]IApplication<T> application, IServiceProvider current = null ) : this( application, new AssignServiceProvider( current ) ) {}

		public ExecuteApplicationCommand( [Required]IApplication<T> application, AssignServiceProvider assign )
		{
			this.application = application;
			this.assign = assign;
		}

		public override void Execute( T parameter )
		{
			assign.Execute( application );
			application.Execute( parameter );
			var repository = application.Get<IDisposableRepository>();
			if ( repository != null )
			{
				application.AssociateForDispose( repository );
			}
		}

		protected override void OnDispose()
		{
			application.Dispose();
			assign.Dispose();
		}
	}

	public class DefaultServiceProvider : ExecutionCachedStoreBase<IServiceProvider>
	{
		public static DefaultServiceProvider Instance { get; } = new DefaultServiceProvider();

		DefaultServiceProvider() : base( () => new ServiceProvider() ) {}
	}

	public class DefaultStoreServiceProvider : StoreServiceProvider
	{
		public static DefaultStoreServiceProvider Instance { get; } = new DefaultStoreServiceProvider();
		DefaultStoreServiceProvider() : base( DefaultServiceProvider.Instance ) {}
	}

	public static class ActivationProperties
	{
		public static ICache<bool> Instance { get; } = new StoreCache<bool>();

		public static ICache<Type> Factory { get; } = new Cache<Type>();

		public class IsActivatedInstanceSpecification : GuardedSpecificationBase<object>
		{
			public static IsActivatedInstanceSpecification Default { get; } = new IsActivatedInstanceSpecification();

			public override bool IsSatisfiedBy( object parameter ) => Instance.Get( parameter ) || new[] { parameter, Factory.Get( parameter ) }.WhereAssigned().Any( o => o.Has<SharedAttribute>() );
		}

		/*public class Instance : AssociatedStore<bool>
		{
			public Instance( object instance ) : base( instance ) {}
		}

		public class Factory : AssociatedStore<Type>
		{
			public Factory( object instance ) : base( instance ) {}
		}*/
	}

	public class InstanceServiceProvider : RepositoryBase<IInstanceRegistration>, IServiceProvider, IServiceRegistry
	{
		readonly ICache<Type, object> cache;

		public InstanceServiceProvider() {}

		public InstanceServiceProvider( IEnumerable<IFactory> factories, params object[] instances ) : this( factories.Select( factory => new DeferredStore( factory ) ).Concat( Instances( instances ) ) ) {}

		public InstanceServiceProvider( params object[] instances ) : this( Instances( instances ) ) {}

		InstanceServiceProvider( IEnumerable<IInstanceRegistration> stores ) : base( stores )
		{
			cache = new Cache<Type, object>( GetServiceBody );
		}

		static IEnumerable<IInstanceRegistration> Instances( IEnumerable<object> instances )
		{
			return instances.Select( o => new InstanceStore( o ) );
			/*var all = instances.ToArray();

			var stores = all.OfType<IStore>().ToArray();
			var references = all.Except( stores ).Select( o => new InstanceStore( o ) );
			var result = stores.Select( store => new InstanceStore( store ) ).Concat( references );
			return result;*/
		}

		public bool IsRegistered( Type type ) => List().Select( registration => registration.RegisteredType ).Any( type.Adapt().IsAssignableFrom );

		void IServiceRegistry.Register( MappingRegistrationParameter parameter )
		{
			throw new NotSupportedException( $"{GetType().Name} does not support type mapping-based service location." );
		}

		public void Register( InstanceRegistrationParameter parameter ) => Add( new InstanceStore( parameter.Instance, parameter.RequestedType ) );

		public void RegisterFactory( FactoryRegistrationParameter parameter ) => Add( new DeferredStore( parameter.Factory, parameter.RequestedType ) );

		public object GetService( Type serviceType ) => cache.Get( serviceType );

		object GetServiceBody( Type serviceType )
		{
			var result = List().Introduce( serviceType.Adapt(), tuple => tuple.Item2.IsAssignableFrom( tuple.Item1.RegisteredType ) ).Select( store => store.Value ).FirstOrDefault();
			if ( result != null )
			{
				ActivationProperties.Instance.Set( result, true );
			}
			return result;
		}
	}

	public interface IInstanceRegistration : IStore
	{
		Type RegisteredType { get; }
	}

	class DeferredStore : DeferredStore<object>, IInstanceRegistration
	{
		public DeferredStore( IFactory factory ) : this( factory.ToDelegate(), ResultTypeLocator.Instance.Get( factory.GetType() ) ) {}

		public DeferredStore( Func<object> factory, Type registeredType ) : base( factory )
		{
			RegisteredType = registeredType;
		}

		public Type RegisteredType { get; }
	}

	class InstanceStore : FixedStore<object>, IInstanceRegistration
	{
		// public InstanceStore( IStore store ) : this( store.Value ) {}

		public InstanceStore( object reference ) : this( reference, reference.GetType() ) {}

		public InstanceStore( object reference, Type registeredType ) : base( reference )
		{
			RegisteredType = registeredType;
		}

		public Type RegisteredType { get; }
	}

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( /*IsServiceTypeSpecification.Instance,*/ providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );
	}

	/*public class IsServiceTypeSpecification : GuardedSpecificationBase<Type>
	{
		public static IsServiceTypeSpecification Instance { get; } = new IsServiceTypeSpecification();

		public override bool IsSatisfiedBy( Type parameter ) => !parameter.GetTypeInfo().IsValueType;
	}*/

	public class RecursionAwareServiceProvider : DecoratedServiceProvider
	{
		readonly IsActive cache = new IsActive();

		public RecursionAwareServiceProvider( IServiceProvider inner ) : base( inner ) {}

		public override object GetService( Type serviceType )
		{
			var active = cache.Get( serviceType );
			if ( !active )
			{
				using ( cache.Assignment( serviceType, true ) )
				{
					return base.GetService( serviceType );
				}
			}

			return null;
		}

		class IsActive : StoreCache<Type, bool>
		{
			public IsActive() : base( new ThreadLocalStoreCache<Type, bool>() ) {}
		}
	}

	public class DecoratedServiceProvider : IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( [Required] Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}

	public abstract class StoreServiceProvider : DecoratedStore<IServiceProvider>, IServiceProvider
	{
		protected StoreServiceProvider( IWritableStore<IServiceProvider> store ) : base( store ) {}

		public object GetService( Type serviceType ) => Value.GetService( serviceType );
	}

	public class ConfiguredServiceProviderFactory<TCommand> : ConfiguringFactory<IServiceProvider> where TCommand : class, ICommand<IServiceProvider>
	{
		public ConfiguredServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider, Configure<TCommand>.Instance.ToDelegate() ) {}
	}

	class Configure<T> : CommandBase<IServiceProvider> where T : class, ICommand<IServiceProvider>
	{
		public static Configure<T> Instance { get; } = new Configure<T>();

		public override void Execute( IServiceProvider parameter ) => parameter.Get<T>().Execute( parameter );
	}

	public interface IApplication<in T> : IApplication, ICommand<T> {}

	public interface IApplication : ICommand, IServiceProvider, IDisposable
	{
		// void Register( IDisposable disposable );
	}

	/*public class FrameworkTypes : FactoryBase<Type[]>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();

		[Freeze]
		protected override Type[] CreateItem() => new[] { typeof(ConfigureProviderCommand) };
	}*/

	public class FrameworkTypes : FixedFactory<ImmutableArray<Type>>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes( typeof(ConfigureProviderCommand), typeof(ParameterInfoFactoryTypeLocator), typeof(MemberInfoFactoryTypeLocator), typeof(ApplicationAssemblyLocator), typeof(MethodFormatter) );
		protected FrameworkTypes( params Type[] types ) : base( types.ToImmutableArray() ) {}
	}

	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application( IServiceProvider provider ) : this( provider, Items<ICommand>.Default ) {}

		protected Application( IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( new ConditionMonitorSpecification<T>(), commands.ToArray() ) {}

		[Required]
		public IServiceProvider Services { [return: Required]get; set; }

		public virtual object GetService( Type serviceType ) => typeof(IApplication).Adapt().IsAssignableFrom( serviceType ) ? this : Services.GetService( serviceType );
	}

	public class ApplyExportedCommandsCommand<T> : DisposingCommand<object> where T : ICommand
	{
		[Required, Service]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		readonly ICollection<T> watching = new Collection<T>();

		public override void Execute( object parameter )
		{
			var exports = Host.GetExports<T>( ContractName ).Fixed();
			watching.AddRange( exports );

			foreach ( var export in exports.Prioritize().ToImmutableArray() )
			{
				export.Execute( parameter );
			}
		}

		protected override void OnDispose()
		{
			watching.Purge().OfType<IDisposable>().Each( obj => obj.Dispose() );
			base.OnDispose();
		}
	}

	public class ApplyTaskMonitorCommand : FixedCommand
	{
		public ApplyTaskMonitorCommand() : base( new AmbientStackCommand<ITaskMonitor>(), new TaskMonitor() ) {}
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup() : this( Items<ICommand>.Default ) {}

		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
