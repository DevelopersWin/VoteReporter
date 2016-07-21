using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Setup
{
	public sealed class InitializeSetupCommand : CompositeCommand
	{
		readonly static AssignValueCommand<TypeSystem> Types = new AssignValueCommand<TypeSystem>( ApplicationTypes.Instance );
		readonly static AssignValueCommand<Func<IServiceProvider>> 
			Apply = new AssignValueCommand<Func<IServiceProvider>>( GlobalServiceProvider.Instance ),
			Seed = new AssignValueCommand<Func<IServiceProvider>>( SeviceProviderFactory.Instance.Seed );
		readonly static AssignConfigurationsCommand<IServiceProvider> Configurations = new AssignConfigurationsCommand<IServiceProvider>( SeviceProviderFactory.Instance.Configurators );
		readonly static Func<IServiceProvider> Default = SeviceProviderFactory.Instance.Create;

		public static InitializeSetupCommand Instance { get; } = new InitializeSetupCommand();
		InitializeSetupCommand() : this( Items<Type>.Default ) {}

		public InitializeSetupCommand( IEnumerable<Assembly> assemblies ) : this( assemblies, DefaultServiceProvider.Instance ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed ) : this( assemblies, seed, SeviceProviderFactory.Default.ToArray() ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( assemblies, seed, configurators, Default ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( assemblies.ToImmutableArray(), seed, configurators, provider ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies ) : this( assemblies, DefaultServiceProvider.Instance ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed ) : this( assemblies, seed, SeviceProviderFactory.Default.ToArray() ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( assemblies, seed, configurators, Default ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( new TypeSystem( assemblies ), seed, configurators, provider ) {}

		public InitializeSetupCommand( IEnumerable<Type> types ) : this( types, DefaultServiceProvider.Instance ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed ) : this( types, seed, SeviceProviderFactory.Default.ToArray() ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( types, seed, configurators, Default ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( types.ToImmutableArray(), seed, configurators, provider ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types ) : this( types, DefaultServiceProvider.Instance ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed ) : this( types, seed, SeviceProviderFactory.Default.ToArray() ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( types, seed, configurators, Default ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( new TypeSystem( types ), seed, configurators, provider ) {}

		public InitializeSetupCommand( TypeSystem typeSystem, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : base( Types.Fixed( typeSystem ), Seed.Fixed( seed.Self ), Configurations.Fixed( configurators.ToImmutableArray() ), Apply.Fixed( provider ) ) {}
	}

	public sealed class SeviceProviderFactory : AggregateFactoryBase<IServiceProvider>
	{
		public static ImmutableArray<ITransformer<IServiceProvider>> Default { get; } = ImmutableArray.Create<ITransformer<IServiceProvider>>( ServiceProviderFactory.Instance );
		public static SeviceProviderFactory Instance { get; } = new SeviceProviderFactory();
		SeviceProviderFactory() : base( () => DefaultServiceProvider.Instance, () => Default ) {}
	}

	/*public sealed class ConfiguredServiceProviderFactory : ConfiguringFactory<TypeSystem, IServiceProvider>
	{
		public static ConfiguredServiceProviderFactory Instance { get; } = new ConfiguredServiceProviderFactory();
		ConfiguredServiceProviderFactory() : base( new Func<IServiceProvider>( GlobalServiceProvider.Instance.Get ).Wrap<TypeSystem, IServiceProvider>().ToDelegate(), InitializeServicesCommand.Instance.Execute ) {}

		public IServiceProvider Create( ImmutableArray<Type> types ) => Create( types.ToArray() );
		public IServiceProvider Create( params Type[] types ) => Create( new TypeSystem( types ) );

		public IServiceProvider Create( ImmutableArray<Assembly> assemblies ) => Create( assemblies.ToArray() );
		public IServiceProvider Create( params Assembly[] assemblies ) => Create( new TypeSystem( assemblies ) );
	}*/

	/*public static class ApplicationExtensions
	{
		public static IApplication<T> AsExecuted<T>( this IApplication<T> @this, T arguments )
		{
			using ( var command = new ExecuteApplicationCommand<T>( @this ) )
			{
				command.Execute( arguments );
			}
			return @this;
		}
	}*/

	/*public class ExecuteApplicationCommand<T> : DisposingCommand<T>
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
	}*/

	public static class ActivationProperties
	{
		public static ICache<bool> Instance { get; } = new StoreCache<bool>();

		public static ICache<Type> Factory { get; } = new Cache<Type>();

		public sealed class IsActivatedInstanceSpecification : GuardedSpecificationBase<object>
		{
			public static IsActivatedInstanceSpecification Default { get; } = new IsActivatedInstanceSpecification();
			IsActivatedInstanceSpecification() {}

			public override bool IsSatisfiedBy( object parameter ) => Instance.Get( parameter ) || new[] { parameter, Factory.Get( parameter ) }.WhereAssigned().Any( o => o.Has<SharedAttribute>() );
		}
	}

	public class InstanceServiceProvider : RepositoryBase<object>, IServiceProvider/*, IServiceRegistry*/
	{
		readonly ICache<Type, object> cache;

		/*public InstanceServiceProvider() {}

		public InstanceServiceProvider( IEnumerable<IFactory> factories, params object[] instances ) : this( factories.Select( factory => new DeferredStore( factory ) ).Concat( Instances( instances ) ) ) {}*/

		public InstanceServiceProvider( params object[] instances ) : base( instances )
		{
			cache = new Cache<Type, object>( GetServiceBody );
		}

		/*static IEnumerable<IInstanceRegistration> Instances( IEnumerable<object> instances )
		{
			return instances.Select( o => new InstanceStore( o ) );
			/*var all = instances.ToArray();

			var stores = all.OfType<IStore>().ToArray();
			var references = all.Except( stores ).Select( o => new InstanceStore( o ) );
			var result = stores.Select( store => new InstanceStore( store ) ).Concat( references );
			return result;#1#
		}*/

		/*public bool IsRegistered( Type type ) => List().Select( registration => registration.RegisteredType ).Any( type.Adapt().IsAssignableFrom );

		void IServiceRegistry.Register( MappingRegistrationParameter parameter )
		{
			throw new NotSupportedException( $"{GetType().Name} does not support type mapping-based service location." );
		}

		public void Register( InstanceRegistrationParameter parameter ) => Add( new InstanceStore( parameter.Instance, parameter.RequestedType ) );

		public void RegisterFactory( FactoryRegistrationParameter parameter ) => Add( new DeferredStore( parameter.Factory, parameter.RequestedType ) );*/

		public object GetService( Type serviceType ) => cache.Get( serviceType );

		object GetServiceBody( Type serviceType )
		{
			var result = List().Introduce( serviceType.Adapt(), tuple => tuple.Item2.IsAssignableFrom( tuple.Item1.GetType() ) ).FirstAssigned();
			if ( result != null )
			{
				ActivationProperties.Instance.Set( result, true );
			}
			return result;
		}
	}

	/*public interface IInstanceRegistration : IStore
	{
		Type RegisteredType { get; }
	}*/

	/*class DeferredStore : DeferredStore<object>, IInstanceRegistration
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
	}*/

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );
	}

	/*public class RecursionAwareServiceProvider : DecoratedServiceProvider
	{
		readonly IsActive active = new IsActive();

		public RecursionAwareServiceProvider( IServiceProvider inner ) : base( inner ) {}

		public override object GetService( Type serviceType )
		{
			var isActive = active.Get( serviceType );
			if ( !isActive )
			{
				using ( active.Assignment( serviceType, true ) )
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
	}*/

	/*public class DecoratedServiceProvider : IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( [Required] Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}*/

	public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static IStore<IRepository<IServiceProvider>> Instance { get; } = new ExecutionContextStore<IRepository<IServiceProvider>>( () => new ServiceProviderRegistry() );
		ServiceProviderRegistry() {}
	}

	public interface IDependencyLocator : ICache<IDependencyLocatorKey, IServiceProvider>
	{
		ServiceLocator For( IDependencyLocatorKey locatorKey );
	}

	public class RegisterServiceProviderCommand : CommandBase<IServiceProvider>
	{
		public static RegisterServiceProviderCommand Instance { get; } = new RegisterServiceProviderCommand();
		RegisterServiceProviderCommand() : this( DependencyLocator.Instance, ServiceProviderRegistry.Instance.Get ) {}

		readonly IDependencyLocator locator;
		readonly Func<IRepository<IServiceProvider>> repository;

		public RegisterServiceProviderCommand( IDependencyLocator locator, Func<IRepository<IServiceProvider>> repository )
		{
			this.locator = locator;
			this.repository = repository;
		}

		public override void Execute( IServiceProvider parameter )
		{
			var key = parameter.Get<IDependencyLocatorKey>();
			if ( key != null && !locator.Contains( key ) )
			{
				repository().Add( locator.SetValue( key, parameter ) );
			}
		}
	}

	public delegate object ServiceLocator( Type serviceType );

	class DependencyLocator : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static IDependencyLocator Instance { get; } = new DependencyLocator();
		DependencyLocator() {}

		readonly ICache<IServiceProvider, ServiceLocator> sources = new Cache<IServiceProvider, ServiceLocator>( provider => ActivatedServiceProvider.Stores.Get( provider ).Value.GetService );

		public ServiceLocator For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? sources.Get( Get( locatorKey ) ) : null;

		interface IServiceProviderStore : IStore<IServiceProvider>, IServiceProvider
		{
			bool CanProvide( Type serviceType );
		}
		
		class ActivatedServiceProvider : FixedStore<IServiceProvider>, IServiceProviderStore
		{
			public static ICache<IServiceProvider, IServiceProviderStore> Stores { get; } = new Cache<IServiceProvider, IServiceProviderStore>( ParameterConstructor<IServiceProvider, ActivatedServiceProvider>.Default );
			ActivatedServiceProvider( IServiceProvider provider ) : base( provider ) {}

			readonly IsActive active = new IsActive();

			public object GetService( Type serviceType )
			{
				using ( active.Assignment( serviceType, true ) )
				{
					return ServiceProviderRegistry.Instance.Value.List().Select( Stores.Get ).Introduce( serviceType, tuple => tuple.Item1.CanProvide( tuple.Item2 ), tuple => tuple.Item1.Value.GetService( tuple.Item2 ) ).FirstAssigned();
				}
			}

			public bool CanProvide( Type serviceType ) => !active.Get( serviceType );

			class IsActive : StoreCache<Type, bool>
			{
				public IsActive() : base( new ThreadLocalStoreCache<Type, bool>() ) {}
			}
		}
	}

	public interface IDependencyLocatorKey {}

	public interface IApplication<in T> : IApplication, ICommand<T> {}

	public interface IApplication : ICommand, IServiceProvider, IDisposable {}

	public struct TypeSystem
	{
		public TypeSystem( ImmutableArray<Assembly> assemblies ) : this( assemblies, TypesFactory.Instance.Create( assemblies.ToArray() ) ) {}

		public TypeSystem( ImmutableArray<Type> types ) : this( types.Assemblies(), types ) {}

		TypeSystem( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
		{
			Assemblies = assemblies;
			Types = types;
		}

		public ImmutableArray<Assembly> Assemblies { get; }
		public ImmutableArray<Type> Types { get; }
	}

	public sealed class ApplicationTypes : ExecutionContextStructureStore<TypeSystem>
	{
		public static ApplicationTypes Instance { get; } = new ApplicationTypes();
		ApplicationTypes() {}
	}

	/*public class AllTypes : ExecutionContextStructureStore<ImmutableArray<Type>>
	{
		public static IStore<ImmutableArray<Type>> Instance { get; } = new AllTypes();
		AllTypes() : base( () => ApplicationTypes.Instance.Get().Types.ToArray().Union( FrameworkTypes.Instance.Get().ToArray() ).ToImmutableArray() ) {}
	}*/

	public class FrameworkTypes : ExecutionContextStructureStore<ImmutableArray<Type>>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();
		FrameworkTypes() : base( () => Default ) {}

		readonly static ImmutableArray<Type> Default = ImmutableArray.Create( typeof(ParameterInfoFactoryTypeLocator), typeof(MemberInfoFactoryTypeLocator), typeof(ApplicationAssemblyLocator), typeof(MethodFormatter) );
	}

	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( Items<ICommand>.Default ) {}

		protected Application( IEnumerable<ICommand> commands ) : base( new OnlyOnceSpecification<T>(), commands.ToArray() ) {}

		[Required]
		public IServiceProvider Services { [return: Required] get; set; } = GlobalServiceProvider.Instance.Get();

		public virtual object GetService( Type serviceType ) => typeof(IApplication).Adapt().IsAssignableFrom( serviceType ) ? this : Services.GetService( serviceType );
	}

	public class ApplyExportedCommandsCommand<T> : DisposingCommand<object> where T : class, ICommand
	{
		[Required, Service]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		readonly ICollection<T> watching = new WeakList<T>();

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

	public class ApplyTaskMonitorCommand : FixedCommand<ITaskMonitor>
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
