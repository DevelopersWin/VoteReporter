using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
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
	/*public class InitializeSystemCommand : AssignValueCommand<SystemParts>
	{
		public static InitializeSystemCommand Instance { get; } = new InitializeSystemCommand();
		InitializeSystemCommand() : base( ApplicationParts.Instance ) {}

		public void Initialize() => Execute( Items<Type>.Immutable );

		public void Execute( IEnumerable<Assembly> assemblies ) => Execute( assemblies.ToImmutableArray() );
		public void Execute( ImmutableArray<Assembly> assemblies ) => Execute( new SystemParts( assemblies ) );

		public void Execute( IEnumerable<Type> types ) => Execute( types.ToImmutableArray() );
		public void Execute( ImmutableArray<Type> types ) => Execute( new SystemParts( types ) );
	}*/

	/*public sealed class InitializeSetupCommand : CompositeCommand
	{
		readonly static AssignValueCommand<Func<IServiceProvider>> 
			Apply = new AssignValueCommand<Func<IServiceProvider>>( GlobalServiceProvider.Instance ),
			Seed = new AssignValueCommand<Func<IServiceProvider>>( ServiceProviderFactory.Instance.Seed );
		readonly static AssignConfigurationsCommand<IServiceProvider> Configurations = new AssignConfigurationsCommand<IServiceProvider>( ServiceProviderFactory.Instance.configurations );
		readonly static Func<IServiceProvider> Default = ServiceProviderFactory.Instance.Create;
		readonly static Func<ITransformer<IServiceProvider>[]> DefaultConfigurators = () => ServiceProviderFactory.Instance.configurations.Get().ToArray();

		public static InitializeSetupCommand Instance { get; } = new InitializeSetupCommand();
		InitializeSetupCommand() : this( Items<Type>.Default ) {}

		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed ) : this( assemblies, seed, DefaultConfigurators() ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, params ITransformer<IServiceProvider>[] configurators ) : this( assemblies, seed, configurators, Default ) {}
		public InitializeSetupCommand( IEnumerable<Assembly> assemblies, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : this( assemblies.ToImmutableArray(), seed, configurators, provider ) {}
		public InitializeSetupCommand(  ) : this( assemblies, DefaultServiceProvider.Instance.Value ) {}
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

		public InitializeSetupCommand( SystemParts systemParts, IServiceProvider seed, IEnumerable<ITransformer<IServiceProvider>> configurators, Func<IServiceProvider> provider ) : base( Parts.Fixed( systemParts ), Seed.Fixed( seed.Self ), Configurations.Fixed( configurators.ToImmutableArray() ), Apply.Fixed( provider ) ) {}
	}*/

	/*public class ConfigureServiceProviderCommand : ConfigureFactoryCommand<IServiceProvider>
	{
		public static ConfigureServiceProviderCommand Instance { get; } = new ConfigureServiceProviderCommand();
		ConfigureServiceProviderCommand() : base( ServiceProviderFactory.Instance/*, GlobalServiceProvider.Instance#1# ) {}
	}

	public class ConfigureFactoryCommand<T> : CommandBase<Parameter<T>>
	{
		readonly IConfigurableFactory<T> source;
		// readonly IConfigurable<T> target;

		public ConfigureFactoryCommand( IConfigurableFactory<T> source/*, IConfigurable<T> target#1# )
		{
			this.source = source;
			// this.target = target;
		}

		public override void Execute( Parameter<T> parameter )
		{
			source.Seed.Assign( parameter.Seed ?? source.Seed.Get );
			source.configurations.Assign( parameter.configurations ?? source.configurations.Get );
			// target.Assign( source.Create );
		}
	}*/

	/*public sealed class ApplicationConfigurator<T> : FactoryBase<ApplicationConfigurationParameter<T>, T> where T : IApplication, new()
	{
		public static ApplicationConfigurator<T> Instance { get; } = new ApplicationConfigurator<T>();
		ApplicationConfigurator() : this( ApplicationConfiguration.Instance, ActiveApplication.Instance ) {}

		readonly IApplicationConfiguration configuration;
		readonly IAssignable<IApplication> active;

		public ApplicationConfigurator( IApplicationConfiguration configuration, IAssignable<IApplication> active )
		{
			this.configuration = configuration;
			this.active = active;
		}

		/*public T Create( IEnumerable<Assembly> assemblies ) => Create( assemblies.ToImmutableArray() );
		public T Create( ImmutableArray<Assembly> assemblies ) => Create( new SystemParts( assemblies ) );

		public T Create( IEnumerable<Type> types ) => Create( types.ToImmutableArray() );
		public T Create( ImmutableArray<Type> types ) => Create( new SystemParts( types ) );

		static T Create( SystemParts parts ) => Instance.Create( new ApplicationConfigurationParameter<T>( parts ) );#1#

		public override T Create( ApplicationConfigurationParameter<T> parameter )
		{
			/*if ( parameter.Parts.IsAssigned() )
			{
				configuration.Parts.Assign( new FixedFactory<SystemParts>( parameter.Parts ).Create );
			}

			if ( parameter.Services != null )
			{
				configuration.Services.Assign( parameter.Services.Self );
			}

			var commands = parameter.Commands.Fixed();
			if ( commands.Any() )
			{
				configuration.Commands.Assign( commands.ToImmutableArray );
			}#1#
			var result = new T();
			active.Assign( result );
			return result;
		}
	}

	public struct ApplicationConfigurationParameter<T> where T : IApplication, new()
	{
		public ApplicationConfigurationParameter( IServiceProvider services = null, params ICommand[] commands ) : this( default(SystemParts), services, commands ) {}

		public ApplicationConfigurationParameter( SystemParts parts = default(SystemParts), IServiceProvider services = null, params ICommand[] commands )
		{
			Parts = parts;
			Services = services;
			Commands = commands;
		}

		public SystemParts Parts { get; }
		public IServiceProvider Services { get; }
		public IEnumerable<ICommand> Commands { get; }
	}*/

	/*public class ConfigurationParameter<T>
	{
		public ConfigurationParameter( Func<ImmutableArray<ITransformer<T>>> configurators = null ) : this( null, configurators ) {}

		public ConfigurationParameter( Func<T> seed = null ) : this( seed, null ) {}

		public ConfigurationParameter( Func<T> seed = null, Func<ImmutableArray<ITransformer<T>>> configurators = null )
		{
			Seed = seed;
			configurations = configurators;
		}

		public Func<T> Seed { get; }
		public Func<ImmutableArray<ITransformer<T>>> configurations { get; }
	}*/

	/*public struct Parameter<T>
	{
		public Parameter( Func<ImmutableArray<ITransformer<T>>> configurators = null ) : this( null, configurators ) {}

		public Parameter( Func<T> seed = null ) : this( seed, null ) {}

		public Parameter( Func<T> seed = null, Func<ImmutableArray<ITransformer<T>>> configurators = null )
		{
			Seed = seed;
			configurations = configurators;
		}

		public Func<T> Seed { get; }
		public Func<ImmutableArray<ITransformer<T>>> configurations { get; }
	}*/

	public interface IExportProvider
	{
		IEnumerable<T> GetExports<T>();
	}

	public class Exports : Configuration<IExportProvider>
	{
		public static Exports Instance { get; } = new Exports();
		Exports() : base( () => ExportProvider.Instance ) {}
	}

	class ExportProvider : IExportProvider
	{
		public static ExportProvider Instance { get; } = new ExportProvider();
		ExportProvider() {}

		public IEnumerable<T> GetExports<T>()
		{
			yield break;
		}
	}

	public interface ICommandSource : ISource<ImmutableArray<ICommand>> {}
	public interface ITypeSource : ISource<ImmutableArray<Type>> {}

	public class TypeSource : ItemsStoreBase<Type>, ITypeSource
	{
		public TypeSource( IEnumerable<Type> items ) : base( items ) {}
		public TypeSource() {}
		public TypeSource( params Type[] items ) : base( items ) {}
	}

	public sealed class ServiceProviderFactory : AggregateFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Instance ) {}
	}

	/*public class ServiceProviderConfigurator : Configurator<IServiceProvider>
	{
		public static ServiceProviderConfigurator Instance { get; } = new ServiceProviderConfigurator();

		protected override IEnumerable<ITransformer<IServiceProvider>> From()
		{
			yield return Composition.ServiceProviderFactory.Instance;
		}
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

	public class SourceInstanceServiceProvider : InstanceServiceProviderBase<ISource>
	{
		public SourceInstanceServiceProvider( params ISource[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().Select( o => o.Get() ).FirstOrDefaultOfType<T>();
	}

	public abstract class InstanceServiceProviderBase<T> : RepositoryBase<T>, IServiceProvider
	{
		readonly ICache<Type, object> cache;
		readonly IGenericMethodContext<Invoke> method;

		protected InstanceServiceProviderBase( params T[] instances ) : base( instances )
		{
			method = new GenericMethodFactories( this )[ nameof(GetService) ];
			cache = new Cache<Type, object>( GetServiceBody );
		}

		public object GetService( Type serviceType ) => cache.Get( serviceType );

		object GetServiceBody( Type serviceType ) => method.Make( serviceType ).Invoke<object>();

		protected abstract TService GetService<TService>();
	}

	public class InstanceServiceProvider : InstanceServiceProviderBase<object>
	{
		public InstanceServiceProvider( params object[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().FirstOrDefaultOfType<T>();
	}

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => /*serviceType == typeof(IServiceProvider) ? this :*/ Create( serviceType );
	}

	public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static ISource<IRepository<IServiceProvider>> Instance { get; } = new ExecutionScope<IRepository<IServiceProvider>>( () => new ServiceProviderRegistry() );
		ServiceProviderRegistry() : base( EnumerableEx.Return( DefaultServiceProvider.Instance ) ) {}
	}

	public interface IDependencyLocator : ICache<IDependencyLocatorKey, IServiceProvider>
	{
		ServiceSource For( IDependencyLocatorKey locatorKey );
	}

	public class RegisterServiceProviderCommand : CommandBase<IServiceProvider>
	{
		public static RegisterServiceProviderCommand Instance { get; } = new RegisterServiceProviderCommand();
		RegisterServiceProviderCommand() : this( DependencyLocators.Instance, ServiceProviderRegistry.Instance.Get ) {}

		readonly IDependencyLocator locator;
		readonly Func<IRepository<IServiceProvider>> repositorySource;

		public RegisterServiceProviderCommand( IDependencyLocator locator, Func<IRepository<IServiceProvider>> repositorySource )
		{
			this.locator = locator;
			this.repositorySource = repositorySource;
		}

		public override void Execute( IServiceProvider parameter )
		{
			var key = parameter.Get<IDependencyLocatorKey>();
			if ( key != null && !locator.Contains( key ) )
			{
				repositorySource().Add( locator.SetValue( key, parameter ) );
			}
		}
	}

	class DependencyLocators : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static IDependencyLocator Instance { get; } = new DependencyLocators();
		DependencyLocators() {}

		readonly ICache<IServiceProvider, ServiceSource> sources = new Cache<IServiceProvider, ServiceSource>( provider => ActivatedServiceProvider.Stores.Get( provider ).GetService );

		public ServiceSource For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? sources.Get( Get( locatorKey ) ) : null;

		interface IServiceProviderStore : IServiceProvider, IParameterizedSource<Type, object>
		{
			bool CanProvide( Type serviceType );
		}
		
		class ActivatedServiceProvider : FixedStore<IServiceProvider>, IServiceProviderStore
		{
			public static ICache<IServiceProvider, IServiceProviderStore> Stores { get; } = new Cache<IServiceProvider, IServiceProviderStore>( provider => new ActivatedServiceProvider( provider ) );
			readonly static Func<IServiceProvider, IServiceProviderStore> Selector = Stores.Get;
			ActivatedServiceProvider( IServiceProvider provider ) : base( provider ) {}

			readonly IsActive active = new IsActive();

			public object GetService( Type serviceType )
			{
				using ( active.Assignment( serviceType, true ) )
				{
					var stores = ServiceProviderRegistry.Instance.Get().List().Select( Selector );
					var result = stores.Introduce( serviceType, tuple => tuple.Item1.CanProvide( tuple.Item2 ), tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
					return result;
				}
			}
			
			public bool CanProvide( Type serviceType ) => !active.Get( serviceType );

			public object Get( Type serviceType )
			{
				using ( active.Assignment( serviceType, true ) )
				{
					return Value.GetService( serviceType );
				}
			}

			class IsActive : StoreCache<Type, bool>
			{
				public IsActive() : base( new ThreadLocalStoreCache<Type, bool>() ) {}
			}

			object IParameterizedSource.Get( object parameter ) => parameter is Type ? Get( (Type)parameter ) : null;
		}
	}

	public interface IDependencyLocatorKey {}

	public interface IApplication<in T> : ICommand<T>, IApplication {}

	public interface IApplication : ICommand, IDisposable {}

	public struct SystemParts
	{
		public static SystemParts Default { get; } = new SystemParts( ImmutableArray<Assembly>.Empty );

		public SystemParts( ImmutableArray<Assembly> assemblies ) : this( assemblies, TypesFactory.Instance.Get( assemblies ) ) {}

		public SystemParts( ImmutableArray<Type> types ) : this( types.Assemblies(), types ) {}

		SystemParts( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
		{
			Assemblies = assemblies;
			Types = types;
		}

		public ImmutableArray<Assembly> Assemblies { get; }
		public ImmutableArray<Type> Types { get; }
	}

	public sealed class ApplicationParts : Configuration<SystemParts>
	{
		public static IConfiguration<SystemParts> Instance { get; } = new ApplicationParts();
		ApplicationParts() : base( () => SystemParts.Default ) {}
	}

	public sealed class ApplicationAssemblies : DelegatedStore<ImmutableArray<Assembly>>
	{
		public static ISource<ImmutableArray<Assembly>> Instance { get; } = new ApplicationAssemblies();
		ApplicationAssemblies() : base( () => ApplicationParts.Instance.Get().Assemblies ) {}
	}

	public sealed class ApplicationTypes : DelegatedStore<ImmutableArray<Type>>, ITypeSource
	{
		public static ITypeSource Instance { get; } = new ApplicationTypes();
		ApplicationTypes() : base( () => ApplicationParts.Instance.Get().Types ) {}
	}

	/*public interface IApplicationConfiguration
	{
		ImmutableArray<ICommand> Commands { get; }
		IServiceProvider Services { get; }
		SystemParts Parts { get; }
	}

	public class ApplicationConfiguration : IApplicationConfiguration
	{
		public static IConfiguration<IApplicationConfiguration> Instance { get; } = new Configuration<IApplicationConfiguration>( () => new ApplicationConfiguration() );
		ApplicationConfiguration() {}
		
		public ImmutableArray<ICommand> Commands { get; } = Items<ICommand>.Immutable;
		public IServiceProvider Services { get; } = DefaultServiceProvider.Instance;
		public SystemParts Parts { get; } = SystemParts.Default;
	}*/

	public sealed class ApplicationServices : ExecutionScope<IApplication>
	{
		public static ApplicationServices Instance { get; } = new ApplicationServices();
		ApplicationServices() {}

		public T Create<T>( ImmutableArray<ICommand> commands ) where T : class, IApplication, new()
		{
			ApplicationCommands.Instance.Assign( commands );

			var result = new T();
			Assign( result );
			return result;
		}
	}

	public class ApplicationFactory<T> : ConfiguringFactory<ImmutableArray<ICommand>, IApplication> where T : IApplication, new()
	{
		public static ApplicationFactory<T> Instance { get; } = new ApplicationFactory<T>();
		ApplicationFactory() : this( array => new T() ) {}

		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory ) : this( factory, ApplicationCommands.Instance.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize ) : this( factory, initialize, ApplicationServices.Instance.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize, Action<IApplication> configure ) : base( factory, initialize, configure ) {}

		public T Create() => (T)Create( Items<ICommand>.Immutable );

		public T Create( ITypeSource types ) => Create( types, Items<ICommand>.Default );

		public T Create( ITypeSource types, params ICommandSource[] sources ) => Create( types, sources.Select( source => source.Get() ).Concat().ToImmutableArray() );

		public T Create( ITypeSource types, params ICommand[] commands ) => Create( types, commands.ToImmutableArray() );
		
		public T Create( ITypeSource types, ImmutableArray<ICommand> commands ) => (T)Create( commands.Insert( 0, new ApplySystemPartsConfiguration( types ) ) );
	}

	public sealed class ConfigureSeedingServiceProvider : ApplyDelegateConfigurationCommand<IServiceProvider>
	{
		public ConfigureSeedingServiceProvider( Func<IServiceProvider> provider ) : base( provider, ServiceProviderFactory.Instance.Seed ) {}
	}

	public abstract class CommandsSource : ItemsStoreBase<ICommand>, ICommandSource
	{
		protected CommandsSource( params ICommandSource[] sources ) : this( sources.Select( source => source.Get() ).Concat() ) {}

		protected CommandsSource( IEnumerable<ICommand> items ) : base( items ) {}
		protected CommandsSource() {}
		protected CommandsSource( params ICommand[] items ) : base( items ) {}
	}

	public class ServiceProviderConfigurations : CommandsSource
	{
		public static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();
		protected ServiceProviderConfigurations() {}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return new ConfigureSeedingServiceProvider( GetProvider );
			yield return GlobalServiceProvider.Instance.From( ServiceProviderFactory.Instance );
		}

		protected virtual IServiceProvider GetProvider() => ServiceProviderFactory.Instance.Seed.Get();
	}

	public class ApplicationCommandsSource : CommandsSource
	{
		public ApplicationCommandsSource() : this( ServiceProviderConfigurations.Instance ) {}
		public ApplicationCommandsSource( params ICommandSource[] sources ) : base( sources ) {}
		public ApplicationCommandsSource( IEnumerable<ICommand> items ) : base( items ) {}
		public ApplicationCommandsSource( params ICommand[] items ) : base( items ) {}

		protected override IEnumerable<ICommand> Yield()
		{
			foreach ( var command in base.Yield() )
			{
				yield return command;
			}

			yield return new ApplySetup();
		}
	}

	public class ApplySystemPartsConfiguration : ApplyConfigurationCommand<SystemParts>
	{
		/*public ApplySystemPartsConfiguration( ImmutableArray<Assembly> assemblies ) : this( new SystemParts( assemblies ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Assembly[] assemblies ) : this( assemblies.ToImmutableArray() ) {}*/
		public ApplySystemPartsConfiguration( ITypeSource types ) : this( new SystemParts( types.Get() ) ) {}
		public ApplySystemPartsConfiguration( ImmutableArray<Type> types ) : this( new SystemParts( types ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Type> types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( SystemParts value ) : base( value, ApplicationParts.Instance ) {}
	}

	public sealed class ApplicationCommands : Configuration<ImmutableArray<ICommand>>
	{
		public static IConfiguration<ImmutableArray<ICommand>> Instance { get; } = new ApplicationCommands();
		ApplicationCommands() : base( () => Items<ICommand>.Immutable ) {}
	}

	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( ApplicationCommands.Instance.Get().ToArray() ) {}

		protected Application( params ICommand[] commands ) : base( new OnlyOnceSpecification<T>(), commands )
		{
			/*Parts = parts;
			Services = services;*/
		}

		/*public SystemParts Parts { get; }
		public IServiceProvider Services { get; }*/

		/*[Required]
		public IServiceProvider Services { [return: Required] get; set; }

		public virtual object GetService( Type serviceType ) => typeof(IApplication).Adapt().IsAssignableFrom( serviceType ) ? this : Services.GetService( serviceType );*/
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

	public interface ISetup : ICommand<object>, IDisposable, IPriorityAware {}

	[ApplyAutoValidation]
	public class Setup : CompositeCommand, ISetup
	{
		public Setup() : this( Items<ICommand>.Default ) {}

		public Setup( params ICommand[] commands ) : base( new OnlyOnceSpecification(), commands ) {}

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public Priority Priority { get; set; } = Priority.Normal;
	}
}
