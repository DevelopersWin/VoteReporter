using DragonSpark.Activation;
using DragonSpark.ComponentModel;
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
		readonly static AssignValueCommand<SystemParts> Types = new AssignValueCommand<SystemParts>( ApplicationParts.Instance );
		readonly static AssignValueCommand<Func<IServiceProvider>> 
			Apply = new AssignValueCommand<Func<IServiceProvider>>( GlobalServiceProvider.Instance ),
			Seed = new AssignValueCommand<Func<IServiceProvider>>( ServiceProviderFactory.Instance.Seed );
		readonly static AssignConfigurationsCommand<IServiceProvider> Configurations = new AssignConfigurationsCommand<IServiceProvider>( ServiceProviderFactory.Instance.Configurators );
		readonly static Func<IServiceProvider> Default = ServiceProviderFactory.Instance.Create;
		readonly static Func<ITransformer<IServiceProvider>[]> DefaultConfigurators = () => ServiceProviderFactory.Instance.Configurators.Get().ToArray();

		public static InitializeSetupCommand Instance { get; } = new InitializeSetupCommand();
		InitializeSetupCommand() : this( Items<Type>.Default ) {}

		public InitializeSetupCommand( IEnumerable<Assembly> assemblies ) : this( assemblies, DefaultServiceProvider.Instance.Value ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed ) : this( assemblies, seed, DefaultConfigurators() ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( assemblies, seed, configurators, Default ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( assemblies.ToImmutableArray(), seed, configurators, provider ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies ) : this( assemblies, DefaultServiceProvider.Instance.Value ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed ) : this( assemblies, seed, DefaultConfigurators() ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( assemblies, seed, configurators, Default ) {}
		public InitializeSetupCommand( ImmutableArray<Assembly> assemblies, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( new SystemParts( assemblies ), seed, configurators, provider ) {}

		public InitializeSetupCommand( IEnumerable<Type> types ) : this( types, DefaultServiceProvider.Instance.Value ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed ) : this( types, seed, DefaultConfigurators() ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( types, seed, configurators, Default ) {}
		public InitializeSetupCommand( IEnumerable<Type> types, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( types.ToImmutableArray(), seed, configurators, provider ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types ) : this( types, DefaultServiceProvider.Instance.Value ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed ) : this( types, seed, DefaultConfigurators() ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( types, seed, configurators, Default ) {}
		public InitializeSetupCommand( ImmutableArray<Type> types, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( new SystemParts( types ), seed, configurators, provider ) {}

		public InitializeSetupCommand( SystemParts systemParts, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : base( Types.Fixed( systemParts ), Seed.Fixed( seed.Self ), Configurations.Fixed( configurators.ToImmutableArray() ), Apply.Fixed( provider ) ) {}
	}

	public sealed class ServiceProviderFactory : AggregateFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Instance.Value, ConfigurationsFactory.Instance.Create ) {}

		public class ConfigurationsFactory : ConfigurationsFactoryBase<IServiceProvider>
		{
			public static ConfigurationsFactory Instance { get; } = new ConfigurationsFactory();

			protected override IEnumerable<ITransformer<IServiceProvider>> From()
			{
				yield return Composition.ServiceProviderFactory.Instance;
			}
		}
	}

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

	/*public static class ActivationProperties
	{
		public static ICache<bool> Instance { get; } = new StoreCache<bool>();

		public static ICache<Type> Factory { get; } = new Cache<Type>();

		public sealed class IsActivatedInstanceSpecification : GuardedSpecificationBase<object>
		{
			public static IsActivatedInstanceSpecification Default { get; } = new IsActivatedInstanceSpecification();
			IsActivatedInstanceSpecification() {}

			public override bool IsSatisfiedBy( object parameter ) => Instance.Get( parameter ) || new[] { parameter, Factory.Get( parameter ) }.WhereAssigned().Any( o => o.Has<SharedAttribute>() );
		}
	}*/

	public class InstanceContainerServiceProvider : InstanceServiceProviderBase
	{
		public InstanceContainerServiceProvider( params object[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().FirstAssigned( Defaults<T>.InstanceCoercer );
	}

	public abstract class InstanceServiceProviderBase : RepositoryBase<object>, IServiceProvider
	{
		readonly IGenericMethodContext<Invoke> method;

		protected InstanceServiceProviderBase( params object[] instances ) : base( instances )
		{
			method = new GenericMethodFactories( this )[ nameof(GetService) ];
		}

		public virtual object GetService( Type serviceType ) => method.Make( serviceType ).Invoke<object>();

		protected abstract T GetService<T>();
	}

	public class InstanceServiceProvider : InstanceServiceProviderBase
	{
		readonly ICache<Type, object> cache;

		public InstanceServiceProvider( params object[] instances ) : base( instances )
		{
			cache = new Cache<Type, object>( base.GetService );
		}

		public override object GetService( Type serviceType ) => cache.Get( serviceType );
		protected override T GetService<T>() => Query().FirstOrDefaultOfType<T>();
	}

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );
	}

	public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static IStore<IRepository<IServiceProvider>> Instance { get; } = new ExecutionContextStore<IRepository<IServiceProvider>>( () => new ServiceProviderRegistry() );
		ServiceProviderRegistry() {}
	}

	public interface IDependencyLocator : ICache<IDependencyLocatorKey, IServiceProvider>
	{
		ServiceSource For( IDependencyLocatorKey locatorKey );
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

	class DependencyLocator : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static IDependencyLocator Instance { get; } = new DependencyLocator();
		DependencyLocator() {}

		readonly ICache<IServiceProvider, ServiceSource> sources = new Cache<IServiceProvider, ServiceSource>( provider => ActivatedServiceProvider.Stores.Get( provider ).Value.GetService );

		public ServiceSource For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? sources.Get( Get( locatorKey ) ) : null;

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

	public struct SystemParts
	{
		public static SystemParts Default { get; } = new SystemParts( ImmutableArray<Assembly>.Empty );

		public SystemParts( ImmutableArray<Assembly> assemblies ) : this( assemblies, TypesFactory.Instance.Create( assemblies.ToArray() ) ) {}

		public SystemParts( ImmutableArray<Type> types ) : this( types.Assemblies(), types ) {}

		SystemParts( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
		{
			Assemblies = assemblies;
			Types = types;
		}

		public ImmutableArray<Assembly> Assemblies { get; }
		public ImmutableArray<Type> Types { get; }
	}

	public sealed class ApplicationParts : ExecutionContextStructureStore<SystemParts>
	{
		public static ApplicationParts Instance { get; } = new ApplicationParts();
		ApplicationParts() : base( () => SystemParts.Default ) {}
	}

	public class ApplicationAssemblies : ExecutionContextStructureStore<ImmutableArray<Assembly>>
	{
		public static IStore<ImmutableArray<Assembly>> Instance { get; } = new ApplicationAssemblies();
		ApplicationAssemblies() : base( () => ApplicationParts.Instance.Get().Assemblies ) {}
	}

	public class ApplicationTypes : ExecutionContextStructureStore<ImmutableArray<Type>>
	{
		public static IStore<ImmutableArray<Type>> Instance { get; } = new ApplicationTypes();
		ApplicationTypes() : base( () => ApplicationParts.Instance.Get().Types ) {}
	}

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
