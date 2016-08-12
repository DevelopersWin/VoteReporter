using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Sources;
using DragonSpark.Sources.Caching;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Setup
{
	public interface IExportProvider
	{
		ImmutableArray<T> GetExports<T>( string name );
	}

	public class Exports : Scope<IExportProvider>
	{
		public static IScope<IExportProvider> Instance { get; } = new Exports();
		Exports() : base( () => DefaultExportProvider.Instance ) {}
	}

	class DefaultExportProvider : IExportProvider
	{
		public static DefaultExportProvider Instance { get; } = new DefaultExportProvider();
		DefaultExportProvider() {}

		public ImmutableArray<T> GetExports<T>( string name ) => Items<T>.Immutable;
	}

	public interface ICommandSource : ISource<ImmutableArray<ICommand>> {}
	public interface ITypeSource : ISource<ImmutableArray<Type>> {}

	public class TypeSource : ItemsStoreBase<Type>, ITypeSource
	{
		public TypeSource( IEnumerable<Type> items ) : base( items.Fixed() ) {}
		public TypeSource() : this( Items<Type>.Default ) {}
		public TypeSource( params Type[] items ) : this( items.Distinct() ) {}
	}

	/*public class CompositeTypeSource : TypeSource
	{
		public CompositeTypeSource( IEnumerable<Type> items ) : base( items.Fixed() ) {}
		public CompositeTypeSource() : this( Items<ITypeSource>.Default ) {}

		public CompositeTypeSource( params ITypeSource[] sources ) : base( sources.Select( source => source.Get() ).Concat() ) {}
	}*/

	public sealed class ServiceProviderFactory : ConfigurableFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Instance ) {}
	}

	/*public class ServiceProviderConfigurator : ConfigurationSource<IServiceProvider>
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
		public static ICache<bool> Instance { get; } = new SourceCache<bool>();

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
		readonly IGenericMethodContext<Invoke> method;

		protected InstanceServiceProviderBase( params T[] instances ) : base( instances )
		{
			method = new GenericMethodFactories( this )[ nameof(GetService) ];
		}

		public virtual object GetService( Type serviceType ) => method.Make( serviceType ).Invoke<object>();

		protected abstract TService GetService<TService>();
	}

	public class InstanceServiceProvider : InstanceServiceProviderBase<object>
	{
		readonly ICache<Type, object> cache;

		public InstanceServiceProvider( params object[] instances ) : base( instances )
		{
			cache = new Cache<Type, object>( base.GetService );
		}

		protected override T GetService<T>() => Query().FirstOrDefaultOfType<T>();

		public override object GetService( Type serviceType ) => cache.Get( serviceType );
	}

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Create( serviceType );
	}

	public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static ISource<IRepository<IServiceProvider>> Instance { get; } = new Scope<IRepository<IServiceProvider>>( Factory.Scope( () => new ServiceProviderRegistry() ) );
		ServiceProviderRegistry() : base( DefaultServiceProvider.Instance.Yield() ) {}

		protected override IEnumerable<IServiceProvider> Query() => base.Query().Reverse();
	}

	public abstract class InitializeServiceProviderCommandBase : Setup
	{
		protected InitializeServiceProviderCommandBase( Coerce<IServiceProvider> coercer ) : base( new DelegatedCommand<IServiceProvider>( RegisterServiceProviderCommand.Instance.Execute, coercer ) ) {}
	}

	[ApplyAutoValidation]
	public class RegisterServiceProviderCommand : CommandBase<IServiceProvider>
	{
		public static RegisterServiceProviderCommand Instance { get; } = new RegisterServiceProviderCommand();
		RegisterServiceProviderCommand() : this( DependencyLocators.Instance.Get, ServiceProviderRegistry.Instance.Get ) {}

		readonly Func<IDependencyLocator> locatorSource;
		readonly Func<IRepository<IServiceProvider>> repositorySource;

		public RegisterServiceProviderCommand( Func<IDependencyLocator> locatorSource, Func<IRepository<IServiceProvider>> repositorySource )
		{
			this.locatorSource = locatorSource;
			this.repositorySource = repositorySource;
		}

		public override void Execute( IServiceProvider parameter )
		{
			var key = parameter.Get<IDependencyLocatorKey>();
			if ( key != null )
			{
				var locator = locatorSource();
				if ( !locator.Contains( key ) )
				{
					repositorySource().Add( locator.SetValue( key, parameter ) );
				}
			}
		}
	}

	public interface IDependencyLocator : ICache<IDependencyLocatorKey, IServiceProvider>
	{
		ServiceSource For( IDependencyLocatorKey locatorKey );
	}

	class DependencyLocators : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static ISource<IDependencyLocator> Instance { get; } = new Scope<IDependencyLocator>( Factory.Scope( () => new DependencyLocators() ) );
		DependencyLocators() {}

		public ServiceSource For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? ActivatedServiceProvider.Sources.Get( Get( locatorKey ) ) : null;
	}

	class ActivatedServiceProvider : IServiceProvider
	{
		readonly static Func<IServiceProvider, IFactory<Type, object>> Selector = ActivatedFactory.Default.Get;

		public static IParameterizedSource<IServiceProvider, ServiceSource> Sources { get; } = new Cache<IServiceProvider, ServiceSource>( provider => new ActivatedServiceProvider( provider ).GetService );
		ActivatedServiceProvider( IServiceProvider provider ) : this( provider, IsActive.Default.Get( provider ) ) {}

		readonly IServiceProvider provider;
		readonly IsActive active;

		ActivatedServiceProvider( IServiceProvider provider, IsActive active )
		{
			this.provider = provider;
			this.active = active;
		}

		public object GetService( Type serviceType )
		{
			using ( active.Assignment( serviceType, true ) )
			{
				var stores = ServiceProviderRegistry.Instance.Get().List().Select( Selector );
				var result = stores.Introduce( serviceType, tuple =>
															{
																var canCreate = tuple.Item1.CanCreate( tuple.Item2 );
																return canCreate;
															}, tuple => tuple.Item1.Create( tuple.Item2 ) ).FirstAssigned();
				return result;
			}
		}
			
		// public bool CanProvide( Type serviceType ) => !active.Get( serviceType );
	}

	[ApplyAutoValidation]
	public class ActivatedFactory : FactoryBase<Type, object>
	{
		public static IParameterizedSource<IServiceProvider, IFactory<Type, object>> Default { get; } = new Cache<IServiceProvider, IFactory<Type, object>>( provider => new ActivatedFactory( provider ) );

		readonly IServiceProvider provider;
		readonly IsActive active;

		ActivatedFactory( IServiceProvider provider ) : this( provider, IsActive.Default.Get( provider ) ) {}

		ActivatedFactory( IServiceProvider provider, IsActive active ) : base( new DelegatedSpecification<Type>( active.Get ).Inverse() )
		{
			this.provider = provider;
			this.active = active;
		}

		public override object Create( Type parameter )
		{
			using ( active.Assignment( parameter, true ) )
			{
				return provider.GetService( parameter );
			}
		}
	}

	public class IsActive : StoreCache<Type, bool>
	{
		public static IParameterizedSource<IServiceProvider, IsActive> Default { get; } = new Cache<IServiceProvider, IsActive>( provider => new IsActive() );
		IsActive() : base( new ThreadLocalStoreCache<Type, bool>() ) {}
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

	public sealed class ApplicationParts : Scope<SystemParts>
	{
		public static IScope<SystemParts> Instance { get; } = new ApplicationParts();
		ApplicationParts() : base( () => SystemParts.Default ) {}
	}

	public sealed class ApplicationAssemblies : DelegatedSource<ImmutableArray<Assembly>>
	{
		public static ISource<ImmutableArray<Assembly>> Instance { get; } = new ApplicationAssemblies();
		ApplicationAssemblies() : base( () => ApplicationParts.Instance.Get().Assemblies ) {}
	}

	public sealed class ApplicationTypes : DelegatedSource<ImmutableArray<Type>>, ITypeSource
	{
		public static ITypeSource Instance { get; } = new ApplicationTypes();
		ApplicationTypes() : base( () => ApplicationParts.Instance.Get().Types ) {}
	}

	public sealed class ApplicationServices : Scope<IApplication>
	{
		public static ApplicationServices Instance { get; } = new ApplicationServices();
		ApplicationServices() {}
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
		
		public T Create( ITypeSource types, ImmutableArray<ICommand> commands ) => (T)Create( commands.Insert( 0, new AssignSystemPartsCommand( types ) ).Add( new DisposeDisposableCommand( Disposables.Instance.Get() ) ) );
	}

	/*public sealed class ConfigureSeedingServiceProvider : ApplyDelegateConfigurationCommand<IServiceProvider>
	{
		public ConfigureSeedingServiceProvider( Func<IServiceProvider> provider ) : base( provider, ServiceProviderFactory.Instance.Seed ) {}
	}*/

	public class CommandsSource : ItemsStoreBase<ICommand>, ICommandSource
	{
		protected CommandsSource() {}

		public CommandsSource( params ICommandSource[] sources ) : this( sources.Select( source => source.Get() ).Concat() ) {}
		public CommandsSource( IEnumerable<ICommand> items ) : base( items ) {}
		public CommandsSource( params ICommand[] items ) : base( items ) {}
	}

	public class ServiceProviderConfigurations : CommandsSource
	{
		public static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();
		protected ServiceProviderConfigurations() {}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return GlobalServiceProvider.Instance.Configured( ServiceProviderFactory.Instance.Fix() );
		}
	}

	public class ApplicationCommandsSource : CommandsSource
	{
		//public ApplicationCommandsSource() : this( ServiceProviderConfigurations.Instance ) {}
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

	public class AssignSystemPartsCommand : DecoratedCommand
	{
		public AssignSystemPartsCommand( IEnumerable<Type> types ) : this( types.ToImmutableArray() ) {}
		public AssignSystemPartsCommand( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public AssignSystemPartsCommand( ImmutableArray<Type> types ) : this( new SystemParts( types ) ) {}
		public AssignSystemPartsCommand( ITypeSource types ) : this( new SystemParts( types.Get() ) ) {}
		public AssignSystemPartsCommand( SystemParts value ) : base( ApplicationParts.Instance.Configured( value ).Cast<object>() ) {}
	}

	public sealed class ApplicationCommands : Scope<ImmutableArray<ICommand>>
	{
		public static IScope<ImmutableArray<ICommand>> Instance { get; } = new ApplicationCommands();
		ApplicationCommands() : base( () => Items<ICommand>.Immutable ) {}
	}

	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( ApplicationCommands.Instance.Get().ToArray() ) {}

		protected Application( params ICommand[] commands ) : base( new OnlyOnceSpecification<T>(), commands ) {}
	}

	public class ApplyExportedCommandsCommand<T> : DisposingCommand<object> where T : class, ICommand
	{
		[Required, Service]
		public IExportProvider Exports { [return: Required]get; set; }

		public string ContractName { get; set; }

		readonly ICollection<T> watching = new WeakList<T>();

		public override void Execute( object parameter )
		{
			// var temp = ServiceProviderRegistry.Instance.Get().List();

			var exports = Exports.GetExports<T>( ContractName );
			watching.AddRange( exports.AsEnumerable() );

			foreach ( var export in exports )
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

		public Setup( params ICommand[] commands ) : this( new OncePerScopeSpecification<object>(), commands ) {}
		public Setup( ISpecification<object> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public Priority Priority { get; set; } = Priority.Normal;
	}
}
