using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Setup
{
	public class ServiceProviderFactory : ConfiguredServiceProviderFactory<ConfigureProviderCommand>
	{
		public ServiceProviderFactory( [Required] Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		public ServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> source ) : this( new Func<IServiceProvider>( new Composition.ServiceProviderFactory( source ).Create ) ) {}

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

		protected override void OnExecute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );

			var assign = new AssignValueCommand<IServiceProvider>( host ).AsExecuted( parameter );
			parameter.Get<IDisposableRepository>().With( repository => repository.Add( assign ) );
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
				command.Run( arguments );
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

		protected override void OnExecute( T parameter )
		{
			assign.Run( application );
			application.Execute( parameter );
			application.Get<IDisposableRepository>().With( application.AssociateForDispose );
		}

		protected override void OnDispose()
		{
			application.Dispose();
			assign.Dispose();
		}
	}

	public class DefaultServiceProvider : ExecutionContextStore<ServiceProvider>
	{
		public static DefaultServiceProvider Instance { get; } = new DefaultServiceProvider( () => new ServiceProvider() );

		DefaultServiceProvider( Func<ServiceProvider> create ) : base( create ) {}
	}

	public static class ActivationProperties
	{
		public class IsActivatedInstanceSpecification : CoercedSpecificationBase<object>
		{
			public static IsActivatedInstanceSpecification Instance { get; } = new IsActivatedInstanceSpecification();

			public override bool IsSatisfiedBy( object parameter ) => new Instance( parameter ).Value || new[] { parameter, new Factory( parameter ).Value }.NotNull().Any( o => o.Has<SharedAttribute>() );
		}

		public class Instance : AssociatedStore<bool>
		{
			public Instance( object instance ) : base( instance ) {}
		}

		public class Factory : AssociatedStore<Type>
		{
			public Factory( object instance ) : base( instance ) {}
		}
	}

	public class InstanceServiceProvider : RepositoryBase<IInstanceRegistration>, IServiceProvider
	{
		public InstanceServiceProvider( IEnumerable<IFactory> factories, params object[] instances ) : this( factories.Select( factory => new FactoryStore( factory ) ).Concat( Instances( instances ) ) ) {}

		public InstanceServiceProvider( params object[] instances ) : this( Instances( instances ) ) {}

		InstanceServiceProvider( IEnumerable<IInstanceRegistration> stores ) : base( stores ) {}

		static IEnumerable<IInstanceRegistration> Instances( IEnumerable<object> instances ) => instances.Select( o => new InstanceStore( o ) );

		[Freeze]
		// [RecursionGuard]
		public object GetService( Type serviceType )
		{
			var result = List().Where( registration => serviceType.Adapt().IsAssignableFrom( registration.RegisteredType ) ).Select( store => store.Value ).FirstOrDefault().WithSelf( o => new ActivationProperties.Instance( o ).Assigned( true ) );
			return result;
		}
	}

	public interface IInstanceRegistration : IStore
	{
		Type RegisteredType { get; }
	}

	class FactoryStore : DeferredInstanceStore<object>, IInstanceRegistration
	{
		public FactoryStore( IFactory factory ) : base( factory.Create )
		{
			RegisteredType = Factory.GetResultType( factory.GetType() );
		}

		public Type RegisteredType { get; }
	}

	class InstanceStore : FixedStore<object>, IInstanceRegistration
	{
		public InstanceStore( object reference ) : base( reference )
		{
			RegisteredType = reference.GetType();
		}

		public Type RegisteredType { get; }
	}

	public class CompositeServiceProvider : FirstFromParameterFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( [Required] params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );
	}

	public class RecursionAwareServiceProvider : DecoratedServiceProvider
	{
		public RecursionAwareServiceProvider( IServiceProvider inner ) : base( inner ) {}

		public override object GetService( Type serviceType )
		{
			var context = new IsActive( this, serviceType );
			if ( !context.Value )
			{
				using ( new AssignValueCommand<bool>( context ).AsExecuted( true ) )
				{
					return base.GetService( serviceType );
				}
			}

			return null;
		}

		class IsActive : ThreadAmbientStore<bool>
		{
			public IsActive( object owner, Type type ) : base( KeyFactory.Instance.CreateUsing( owner, type ).ToString() ) {}
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

	public class ConfiguredServiceProviderFactory<TCommand> : ConfiguringFactory<IServiceProvider> where TCommand : class, ICommand<IServiceProvider>
	{
		public ConfiguredServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider, Configure<TCommand>.Instance.Run ) {}
	}

	class Configure<T> : CommandBase<IServiceProvider> where T : class, ICommand<IServiceProvider>
	{
		public static Configure<T> Instance { get; } = new Configure<T>();

		protected override void OnExecute( IServiceProvider parameter ) => parameter.Get<T>().Run( parameter );
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

	public class FrameworkTypes : FactoryBase<Type[]>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();

		[Freeze]
		protected override Type[] CreateItem() => new[] { typeof(ConfigureProviderCommand), typeof(ParameterInfoFactoryTypeLocator), typeof(MemberInfoFactoryTypeLocator), typeof(ApplicationAssemblyLocator), typeof(MethodFormatter) };
	}

	public abstract class Application<TParameter> : CompositeCommand<TParameter>, IApplication<TParameter>
	{
		protected Application( [Required]IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		protected Application( [Required]IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( new OnlyOnceSpecification().Box<TParameter>(), commands.ToArray() ) {}

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

		protected override void OnExecute( object parameter )
		{
			var exports = Host.GetExports<T>( ContractName ).Fixed();
			watching.AddRange( exports );

			exports
				.Prioritize()
				.ExecuteMany( parameter );
		}

		protected override void OnDispose()
		{
			watching.Purge().OfType<IDisposable>().Each( obj => obj.Dispose() );
			base.OnDispose();
		}
	}

	public class ApplyTaskMonitorCommand : FixedCommand
	{
		public ApplyTaskMonitorCommand() : base( new AmbientContextCommand<ITaskMonitor>(), new TaskMonitor() ) {}
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup() : this( Default<ICommand>.Items ) {}

		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
