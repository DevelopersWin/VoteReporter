﻿using DragonSpark.Activation.Location;
using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Commands;
using DragonSpark.Specifications;

namespace DragonSpark.Setup
{
	public interface IExportProvider
	{
		ImmutableArray<T> GetExports<T>( string name = null );
	}

	public class Exports : Scope<IExportProvider>
	{
		public static IScope<IExportProvider> Default { get; } = new Exports();
		Exports() : base( () => DefaultExportProvider.Default ) {}
	}

	class DefaultExportProvider : IExportProvider
	{
		public static DefaultExportProvider Default { get; } = new DefaultExportProvider();
		DefaultExportProvider() {}

		public ImmutableArray<T> GetExports<T>( string name = null ) => Items<T>.Immutable;
	}

	public interface ICommandSource : IItemSource<ICommand> {}
	public interface ITypeSource : IItemSource<Type> {}

	public class TypeSource : ItemSource<Type>, ITypeSource
	{
		public TypeSource() {}
		public TypeSource( params Type[] items ) : base( items ) {}
		public TypeSource( IEnumerable<Type> items ) : base( items ) {}
	}

	public sealed class ServiceProviderFactory : ConfigurableFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Default { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Default ) {}

		public override IServiceProvider Get() => base.Get().Cached();
	}

	public sealed class Instances : Scope<IServiceRepository>
	{
		public static ISource<IServiceRepository> Default { get; } = new Instances();
		Instances() : base( Factory.Global( () => new InstanceServiceProvider( SingletonLocator.Default ) ) ) {}

		public static T Get<T>( Type type ) => Default.Get().Get<T>( type );
	}

	public sealed class RegisterInstanceCommand : CommandBase<object>
	{
		public static RegisterInstanceCommand Default { get; } = new RegisterInstanceCommand();
		RegisterInstanceCommand() : this( Instances.Default.Get ) {}

		readonly Func<IServiceRepository> repository;

		public RegisterInstanceCommand( Func<IServiceRepository> repository )
		{
			this.repository = repository;
		}

		public override void Execute( object parameter ) => repository().Add( parameter );
	}

	public class SourceServiceProvider : InstanceServiceProviderBase<ISource>
	{
		public SourceServiceProvider( params ISource[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().Select( o => o.Get() ).FirstOrDefaultOfType<T>();
	}

	/*public class InstanceRegistrationRequest<T> : InstanceRegistrationRequest
	{
		public InstanceRegistrationRequest( T instance, string name = null ) : base( typeof(T), instance, name ) {}
	}*/

	public class InstanceRegistrationRequest : LocateTypeRequest
	{
		public InstanceRegistrationRequest( object instance, string name = null ) : this( instance.GetType(), instance, name ) {}

		public InstanceRegistrationRequest( Type type, object instance, string name = null ) : base( type, name )
		{
			Instance = instance;
		}

		public object Instance { get; }
	}

	public interface IServiceRepository : IServiceRepository<object>
	{
		void Add( InstanceRegistrationRequest request );
	}

	public interface IServiceRepository<T> : IServiceProvider, IRepository<T>, ISpecification<Type> {}

	public abstract class InstanceServiceProviderBase<T> : RepositoryBase<T>, IServiceRepository<T>
	{
		readonly IGenericMethodContext<Invoke> method;

		protected InstanceServiceProviderBase( params T[] instances ) : base( instances.AsEnumerable() )
		{
			method = new GenericMethodFactories( this )[ nameof(GetService) ];
		}

		public virtual object GetService( Type serviceType ) => method.Make( serviceType ).Invoke<object>();

		protected abstract TService GetService<TService>();

		public bool IsSatisfiedBy( Type parameter ) => Query().Cast<object>().Any( parameter.Adapt().IsInstanceOfType );

		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}

	public class InstanceServiceProvider : InstanceServiceProviderBase<object>, IServiceRepository
	{
		public InstanceServiceProvider() : this( Items<object>.Default ) {}
		public InstanceServiceProvider( params object[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().FirstOrDefaultOfType<T>();
		public virtual void Add( InstanceRegistrationRequest request ) => Add( request.Instance );
	}

	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}

	/*public class ServiceProviderRegistry : RepositoryBase<IServiceProvider>
	{
		public static ISource<IRepository<IServiceProvider>> Default { get; } = new Scope<IRepository<IServiceProvider>>( Factory.ForGlobalScope( () => new ServiceProviderRegistry() ) );
		ServiceProviderRegistry() : base( DefaultServiceProvider.Default.Yield() ) {}

		protected override IEnumerable<IServiceProvider> Query() => base.Query().Reverse();
	}*/

	/*public abstract class InitializeServiceProviderCommandBase : Setup
	{
		protected InitializeServiceProviderCommandBase( Coerce<IServiceProvider> coercer ) : base( new DelegatedCommand<IServiceProvider>( RegisterServiceProviderCommand.Default.Execute, coercer ) ) {}
	}
*/

	[Export( typeof(ISetup) )]
	public sealed class EnableServicesCommand : Setup
	{
		public EnableServicesCommand() : base( Sources.Extensions.Configured( Services.Default, true ) )
		{
			Priority = Priority.High;
		}
	}

	public sealed class Services : Scope<bool>
	{
		public static Services Default { get; } = new Services();
		Services() {}
	}

	/*[ApplyAutoValidation]
	public class RegisterServiceProviderCommand : CommandBase<IServiceProvider>
	{
		public static RegisterServiceProviderCommand Default { get; } = new RegisterServiceProviderCommand();
		RegisterServiceProviderCommand() : this( DependencyLocators.Default.Get, ServiceProviderRegistry.Default.Get ) {}

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
		Func<Type, object> For( IDependencyLocatorKey locatorKey );
	}

	class DependencyLocators : Cache<IDependencyLocatorKey, IServiceProvider>, IDependencyLocator
	{
		public static ISource<IDependencyLocator> Default { get; } = new Scope<IDependencyLocator>( Factory.ForGlobalScope( () => new DependencyLocators() ) );
		DependencyLocators() {}

		public Func<Type, object> For( IDependencyLocatorKey locatorKey ) => Contains( locatorKey ) ? ActivatedServiceProvider.Sources.Get( Get( locatorKey ) ) : null;
	}

	

	public interface IDependencyLocatorKey {}*/

	/*	class ActivatedServiceProvider : IServiceProvider
	{
		readonly static Func<IServiceProvider, IValidatedParameterizedSource<Type, object>> Selector = ActivatedFactory.Default.Get;

		// public static IParameterizedSource<IServiceProvider, Func<Type, object>> Sources { get; } = new Cache<IServiceProvider, Func<Type, object>>( provider => new ActivatedServiceProvider( provider ).GetService );
		ActivatedServiceProvider( IServiceProvider provider ) : this( IsActive.Default.Get( provider ) ) {}

		readonly IsActive active;

		ActivatedServiceProvider( IsActive active )
		{
			this.active = active;
		}

		public object GetService( Type serviceType )
		{
			var stores = ServiceProviderRegistry.Default.Get().List().Select( Selector );
			var result = stores.Introduce( serviceType, tuple => tuple.Item1.IsSatisfiedBy( tuple.Item2 ), tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
			return result;
		}
			
		// public bool CanProvide( Type serviceType ) => !active.Get( serviceType );
	}
*/

	public class IsActive : DecoratedSourceCache<Type, bool>
	{
		public static IParameterizedSource<IServiceProvider, IsActive> Default { get; } = new Cache<IServiceProvider, IsActive>( provider => new IsActive() );
		IsActive() : base( new ThreadLocalSourceCache<Type, bool>() ) {}
	}

	public interface IApplication<in T> : ICommand<T>, IApplication {}

	public interface IApplication : ICommand, IDisposable {}

	public struct SystemParts
	{
		public static SystemParts Default { get; } = new SystemParts( Items<Assembly>.Default );

		public SystemParts( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) {}

		SystemParts( ImmutableArray<Assembly> assemblies ) : this( assemblies, TypesFactory.Default.Get( assemblies ) ) {}

		public SystemParts( IEnumerable<Type> types ) : this( types.Fixed() ) {}

		SystemParts( Type[] types ) : this( types.Assemblies().ToImmutableArray(), types.ToImmutableArray() ) {}

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
		public static IScope<SystemParts> Default { get; } = new ApplicationParts();
		ApplicationParts() : base( () => SystemParts.Default ) {}
	}

	public sealed class ApplicationAssemblies : DelegatedSource<ImmutableArray<Assembly>>
	{
		public static ISource<ImmutableArray<Assembly>> Default { get; } = new ApplicationAssemblies();
		ApplicationAssemblies() : base( () => ApplicationParts.Default.Get().Assemblies ) {}
	}

	public sealed class ApplicationTypes : DelegatedSource<ImmutableArray<Type>>
	{
		public static ISource<ImmutableArray<Type>> Default { get; } = new ApplicationTypes();
		ApplicationTypes() : base( () => ApplicationParts.Default.Get().Types ) {}
	}

	public sealed class ApplicationServices : Scope<IApplication>
	{
		public static ApplicationServices Default { get; } = new ApplicationServices();
		ApplicationServices() {}
	}

	public class ApplicationFactory<T> : ConfiguringFactory<ImmutableArray<ICommand>, IApplication> where T : IApplication, new()
	{
		public static ApplicationFactory<T> Default { get; } = new ApplicationFactory<T>();
		ApplicationFactory() : this( array => new T() ) {}

		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory ) : this( factory, ApplicationCommands.Default.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize ) : this( factory, initialize, ApplicationServices.Default.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize, Action<IApplication> configure ) : base( factory, initialize, configure ) {}

		public T Create() => (T)Get( Items<ICommand>.Immutable );

		public T Create( ITypeSource types ) => Create( types, Items<ICommand>.Default );

		// public T Create( ITypeSource types, params ICommandSource[] sources ) => Create( types, sources.Select( source => source.Get() ).Concat() );

		public T Create( ITypeSource types, params ICommand[] commands ) => Create( types, commands.AsEnumerable() );
		
		public T Create( ITypeSource types, IEnumerable<ICommand> commands ) => (T)Get( commands.StartWith( new AssignSystemPartsCommand( types ) ).Append( new DisposeDisposableCommand( Disposables.Default.Get() ) ).Distinct().Prioritize().ToImmutableArray() );
	}

	public class CommandSource : ItemSource<ICommand>, ICommandSource
	{
		protected CommandSource() {}
		public CommandSource( params ICommandSource[] sources ) : this( sources.Concat() ) {}
		public CommandSource( IEnumerable<ICommand> items ) : base( items ) {}
		public CommandSource( params ICommand[] items ) : base( items ) {}
	}

	public class ServiceProviderConfigurations : CommandSource
	{
		public static ServiceProviderConfigurations Default { get; } = new ServiceProviderConfigurations();
		protected ServiceProviderConfigurations() {}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return GlobalServiceProvider.Default.Configured( ServiceProviderFactory.Default.ToFixedDelegate() );
		}
	}

	public class ApplicationCommandSource : CommandSource
	{
		public ApplicationCommandSource( params ICommandSource[] sources ) : base( sources ) {}
		public ApplicationCommandSource( IEnumerable<ICommand> items ) : base( items ) {}
		public ApplicationCommandSource( params ICommand[] items ) : base( items ) {}

		protected override IEnumerable<ICommand> Yield() => base.Yield().Append( new ApplySetup() );
	}

	public class AssignSystemPartsCommand : DecoratedCommand
	{
		public AssignSystemPartsCommand( params Type[] types ) : this( types.AsEnumerable() ) {}
		//public AssignSystemPartsCommand( ITypeSource types ) : this( types.AsEnumerable() ) {}
		public AssignSystemPartsCommand( IEnumerable<Type> types ) : this( SystemPartsFactory.Default.Get( types ) ) {}
		AssignSystemPartsCommand( SystemParts value ) : base( ApplicationParts.Default.Configured( value ).Cast<object>() ) {}
	}

	public sealed class SystemPartsFactory : ParameterizedSourceBase<IEnumerable<Type>, SystemParts>
	{
		public static SystemPartsFactory Default { get; } = new SystemPartsFactory();
		SystemPartsFactory() {}

		public override SystemParts Get( IEnumerable<Type> parameter ) => new SystemParts( parameter.Distinct().Prioritize( type => AssemblyPriorityLocator.Default.Get( type.Assembly() ) ) );
	}

	public sealed class ApplicationCommands : Scope<ImmutableArray<ICommand>>
	{
		public static IScope<ImmutableArray<ICommand>> Default { get; } = new ApplicationCommands();
		ApplicationCommands() : base( () => Items<ICommand>.Immutable ) {}
	}

	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( ApplicationCommands.Default.Get().ToArray() ) {}

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
