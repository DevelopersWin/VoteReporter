using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
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
	public class AssignApplication : AssignValueCommand<IApplication>
	{
		public AssignApplication() : this( new CurrentApplication() ) {}

		public AssignApplication( IWritableValue<IApplication> value ) : base( value ) {}
	}

	public class CurrentApplication : ExecutionContextValue<IApplication> {}

	public class ApplicationServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<Assembly[]> assemblies;
		readonly Func<Assembly[], CompositionHost> compositionHostFactory;
		readonly Func<ServiceProviderParameter, IServiceLocator> serviceLocatorFactory;

		// public ApplicationServiceProviderFactory( [Required]IAssemblyProvider assemblyProvider, [Required]CompositionHostFactory compositionHostFactory, [Required]Activation.IoC.ServiceLocatorFactory serviceLocatorFactory ) : this( assemblyProvider.Create, compositionHostFactory.Create, serviceLocatorFactory.Create ) {}

		public ApplicationServiceProviderFactory( [Required]Func<Assembly[]> assemblies, [Required]Func<Assembly[], CompositionHost> compositionHostFactory, [Required]Func<ServiceProviderParameter, IServiceLocator> serviceLocatorFactory )
		{
			this.assemblies = assemblies;
			this.compositionHostFactory = compositionHostFactory;
			this.serviceLocatorFactory = serviceLocatorFactory;
		}

		protected override IServiceProvider CreateItem()
		{
			var instance = assemblies();
			var host = compositionHostFactory( instance );
			var parameter = new ServiceProviderParameter( host, instance );
			var serviceLocator = serviceLocatorFactory( parameter );
			var result = new ApplicationServiceProvider( serviceLocator );
			return result;
		}
	}

	public class UnityContainerFactory : FactoryBase<ServiceProviderParameter, IUnityContainer>
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		readonly Func<IUnityContainer> create;

		public UnityContainerFactory() : this( () => new UnityContainer() ) {}

		public UnityContainerFactory( [Required]Func<IUnityContainer> create )
		{
			this.create = create;
		}

		protected override IUnityContainer CreateItem( ServiceProviderParameter parameter )
		{
			var result = create()
				.RegisterInstance( parameter.Assemblies )
				.RegisterInstance( parameter.Host )
				.Extend<DefaultRegistrationsExtension>()
				.Extend<BuildPipelineExtension>()
				.Extend<InstanceTypeRegistrationMonitorExtension>()
				.Extend<CompositionExtension>()
				;
			return result;
		}
	}

	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionHost host;

		public ServiceLocator( [Required]CompositionHost host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports(serviceType, null);

		protected override object DoGetInstance(Type serviceType, string key)
		{
			object item;
			var result = host.TryGetExport( serviceType, key, out item ) ? item : null;
			return result;

			/*throw new ActivationException(
				FormatActivationExceptionMessage(new CompositionFailedException("Export not found"), serviceType, key));*/
		}
	}

	public class ServiceProviderParameter
	{
		public ServiceProviderParameter( [Required]CompositionHost host, Assembly[] assemblies )
		{
			Host = host;
			Assemblies = assemblies;
		}

		public CompositionHost Host { get; }
		public Assembly[] Assemblies { get; }
	}

	public class ServiceLocatorFactory : FactoryBase<ServiceProviderParameter, IServiceLocator>
	{
		public static ServiceLocatorFactory Instance { get; } = new ServiceLocatorFactory();

		protected override IServiceLocator CreateItem( ServiceProviderParameter parameter )
		{
			var registry = parameter.Host.GetExport<IExportDescriptorProviderRegistry>();
			var activator = new CompositeActivator( new SingletonActivator( parameter.Host.TryGet<ISingletonLocator>() ), SystemActivator.Instance );
			registry.Register( new InstanceExportDescriptorProvider<IActivator>( activator ) );
			
			var result = new ServiceLocator( parameter.Host );
			registry.Register( new InstanceExportDescriptorProvider<IServiceLocator>( result ) );
			return result;
		}

		class SingletonActivator : IActivator
		{
			readonly ISingletonLocator locator;

			public SingletonActivator( [Required]ISingletonLocator locator )
			{
				this.locator = locator;
			}

			public bool CanActivate( Type type, string name = null )
			{
				var canActivate = locator.Locate( type ) != null;
				return canActivate;
			}

			public object Activate( Type type, string name = null )
			{
				var activate = locator.Locate( type );
				return activate;
			}

			public bool CanConstruct( Type type, params object[] parameters ) => false;

			public object Construct( Type type, params object[] parameters ) => null;
		}
	}

	class ApplicationServiceProvider : IServiceProvider
	{
		readonly IServiceLocator locator;
		readonly IActivator activator;

		public ApplicationServiceProvider( [Required]IServiceLocator locator ) : this( locator, locator.GetInstance<IActivator>() ) {}

		public ApplicationServiceProvider( [Required]IServiceLocator locator, [Required]IActivator activator )
		{
			this.locator = locator;
			this.activator = activator;
		}

		public object GetService( Type serviceType ) => locator.GetInstance( serviceType ) ?? activator.Activate( serviceType );
	}

	public interface IApplication : ICommand, IServiceProvider, IDisposable {}

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication
	{
		protected Application( [Required]IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( new DecoratedSpecification<TParameter>( new OnlyOnceSpecification() ), commands.ToArray() ) {}

		[Default( true )]
		public bool DisposeAfterExecution { get; set; }

		protected override void OnExecute( TParameter parameter )
		{
			var core = new ICommand[]
			{
				new FixedCommand( new AssignApplication(), () => this ),
				new FixedCommand( new AmbientContextCommand<ITaskMonitor>(), () => new TaskMonitor() )
			};

			core.Each( Commands.Insert );

			base.OnExecute( parameter );

			DisposeAfterExecution.IsTrue( Dispose );
		}

		[Required]
		public IServiceProvider Services { [return: Required]get; set; }

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose()
		{
			Commands.OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
			Commands.Clear();
		}

		public object GetService( Type serviceType ) => Services.GetService( serviceType );
	}

	public class ApplyExportedCommandsCommand<T> : Command<object> where T : ICommand
	{
		[Required, ApplicationService]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		protected override void OnExecute( object parameter ) => 
			Host.GetExports<T>( ContractName )
				.Prioritize()
				.Each( setup => setup.ExecuteWith( parameter ) );
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
