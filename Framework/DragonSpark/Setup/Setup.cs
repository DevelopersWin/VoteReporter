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
	public sealed class InitializeCommand : CompositeCommand
	{
		readonly static AssignValueCommand<TypeSystem> Types = new AssignValueCommand<TypeSystem>( ApplicationTypes.Instance );
		readonly static AssignValueCommand<Func<IServiceProvider>> Provider = new AssignValueCommand<Func<IServiceProvider>>( GlobalServiceProvider.Instance );
		readonly static Func<IServiceProvider> Default = ServiceProviderFactory.Instance.Create;

		public static InitializeCommand Instance { get; } = new InitializeCommand();
		InitializeCommand() : this( Items<Type>.Default ) {}

		public InitializeCommand( IEnumerable<Assembly> assemblies ) : this( assemblies, Default ) {}
		public InitializeCommand( IEnumerable<Assembly> assemblies, Func<IServiceProvider> provider ) : this( assemblies.ToImmutableArray(), provider ) {}
		public InitializeCommand( ImmutableArray<Assembly> assemblies ) : this( assemblies, Default ) {}
		public InitializeCommand( ImmutableArray<Assembly> assemblies, Func<IServiceProvider> provider ) : this( new TypeSystem( assemblies ), provider ) {}

		public InitializeCommand( IEnumerable<Type> types ) : this( types, Default ) {}
		public InitializeCommand( IEnumerable<Type> types, Func<IServiceProvider> provider ) : this( types.ToImmutableArray(), provider ) {}
		public InitializeCommand( ImmutableArray<Type> types ) : this( types, Default ) {}
		public InitializeCommand( ImmutableArray<Type> types, Func<IServiceProvider> provider ) : this( new TypeSystem( types ), provider ) {}
		public InitializeCommand( TypeSystem typeSystem, Func<IServiceProvider> provider ) : base( Types.Fixed( typeSystem ), Provider.Fixed( provider ) ) {}
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
		readonly IsActive active = new IsActive();

		public RecursionAwareServiceProvider( IServiceProvider inner ) : base( inner ) {}

		public override object GetService( Type serviceType )
		{
			var active = this.active.Get( serviceType );
			if ( !active )
			{
				using ( this.active.Assignment( serviceType, true ) )
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

	/*public abstract class StoreServiceProvider : DecoratedStore<IServiceProvider>, IServiceProvider
	{
		protected StoreServiceProvider( IWritableStore<IServiceProvider> store ) : base( store ) {}

		public object GetService( Type serviceType ) => Value.GetService( serviceType );
	}*/

	/*public class ConfiguredServiceProviderFactory<TCommand> : ConfiguringFactory<IServiceProvider> where TCommand : class, ICommand<IServiceProvider>
	{
		public ConfiguredServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider, Configure<TCommand>.Instance.ToDelegate() ) {}
	}

	class Configure<T> : CommandBase<IServiceProvider> where T : class, ICommand<IServiceProvider>
	{
		public static Configure<T> Instance { get; } = new Configure<T>();

		public override void Execute( IServiceProvider parameter ) => parameter.Get<T>().Execute( parameter );
	}*/

	public interface IApplication<in T> : IApplication, ICommand<T> {}

	public interface IApplication : ICommand, IServiceProvider, IDisposable {}

	public struct TypeSystem
	{
		// public TypeSystem( params Assembly[] assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public TypeSystem( ImmutableArray<Assembly> assemblies ) : this( assemblies, TypesFactory.Instance.Create( assemblies.ToArray() ) ) {}

		// public TypeSystem( params Type[] types ) : this( types.ToImmutableArray() ) {}
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

	public class AllTypes : Configuration<ImmutableArray<Type>>
	{
		public static AllTypes Instance { get; } = new AllTypes();
		AllTypes() : base( () => ApplicationTypes.Instance.Get().Types.ToArray().Union( FrameworkTypes.Instance.Get().ToArray() ).ToImmutableArray() ) {}
	}

	public class FrameworkTypes : Configuration<ImmutableArray<Type>>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();
		FrameworkTypes() : base( () => Default ) {}

		readonly static ImmutableArray<Type> Default = ImmutableArray.Create( /*typeof(ConfigureProviderCommand),*/ typeof(ParameterInfoFactoryTypeLocator), typeof(MemberInfoFactoryTypeLocator), typeof(ApplicationAssemblyLocator), typeof(MethodFormatter) );
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
