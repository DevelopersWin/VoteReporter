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

	/*public class CompositionHost : FixedValue<Assembly[]>
	{
		readonly Func<Assembly[], System.Composition.Hosting.CompositionHost> factory;
		readonly CompositionHostContext context;

		public CompositionHost() : this( CompositionHostFactory.Instance.Create, new CompositionHostContext() ) {}

		public CompositionHost( [Required]Func<Assembly[], System.Composition.Hosting.CompositionHost> factory, [Required]CompositionHostContext context )
		{
			this.factory = factory;
			this.context = context;
		}

		public override void Assign( Assembly[] item )
		{
			var host =  item.With( factory );
			context.Assign( host );

			base.Assign( item );
		}
	}*/

	/*public interface IApplicationContext : IServiceProvider
	{
		Assembly[] Assemblies { get; }
	}*/

	public class ApplicationServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<Assembly[]> assemblies;
		readonly Func<Assembly[], CompositionHost> compositionHostFactory;
		readonly Func<ServiceLocatorFactory.Parameter, IServiceLocator> serviceLocatorFactory;

		public ApplicationServiceProviderFactory( [Required]IAssemblyProvider assemblyProvider, [Required]CompositionHostFactory compositionHostFactory, [Required]ServiceLocatorFactory serviceLocatorFactory ) : this( assemblyProvider.Create, compositionHostFactory.Create, serviceLocatorFactory.Create ) {}

		public ApplicationServiceProviderFactory( [Required]Func<Assembly[]> assemblies, [Required]Func<Assembly[], CompositionHost> compositionHostFactory, [Required]Func<ServiceLocatorFactory.Parameter, IServiceLocator> serviceLocatorFactory )
		{
			this.assemblies = assemblies;
			this.compositionHostFactory = compositionHostFactory;
			this.serviceLocatorFactory = serviceLocatorFactory;
		}

		protected override IServiceProvider CreateItem()
		{
			var instance = assemblies();
			var host = compositionHostFactory( instance );
			var parameter = new ServiceLocatorFactory.Parameter( host, instance );
			var serviceLocator = serviceLocatorFactory( parameter );
			var result = new ApplicationServiceProvider( serviceLocator );
			return result;
		}
	}

	public class UnityContainerFactory : FactoryBase<ServiceLocatorFactory.Parameter, IUnityContainer>
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		readonly Func<IUnityContainer> create;

		public UnityContainerFactory() : this( () => new UnityContainer() ) {}

		public UnityContainerFactory( [Required]Func<IUnityContainer> create )
		{
			this.create = create;
		}

		protected override IUnityContainer CreateItem( ServiceLocatorFactory.Parameter parameter )
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
			object result;
			if ( host.TryGetExport( serviceType, key, out result ) )
			{
				return result;
			}

			throw new ActivationException(
				FormatActivationExceptionMessage(new CompositionFailedException("Export not found"), serviceType, key));
		}
	}

	public class DefaultServiceLocatorFactory : FactoryBase<ServiceLocatorFactory.Parameter, IServiceLocator>
	{
		public static DefaultServiceLocatorFactory Instance { get; } = new DefaultServiceLocatorFactory();

		protected override IServiceLocator CreateItem( ServiceLocatorFactory.Parameter parameter )
		{
			var result = new ServiceLocator( parameter.Host );
			var registry = parameter.Host.GetExport<IExportDescriptorProviderRegistry>();
			registry.Register( new InstanceExportDescriptorProvider<IServiceLocator>( result ) );
			registry.Register( new InstanceExportDescriptorProvider<IActivator>( SystemActivator.Instance ) );
			return result;
		}
	}

	public class ServiceLocatorFactory : FactoryBase<ServiceLocatorFactory.Parameter, IServiceLocator>
	{
		public class Parameter
		{
			public Parameter( [Required]CompositionHost host, Assembly[] assemblies )
			{
				Host = host;
				Assemblies = assemblies;
			}

			public CompositionHost Host { get; }
			public Assembly[] Assemblies { get; }
		}

		public static ServiceLocatorFactory Instance { get; } = new ServiceLocatorFactory( AssignLocationCommand.Instance );

		readonly ConfigureLocationCommand configure;
		readonly UnityContainerFactory factory;

		public ServiceLocatorFactory( ConfigureLocationCommand configure ) : this( configure, UnityContainerFactory.Instance ) {}

		public ServiceLocatorFactory( ConfigureLocationCommand configure, UnityContainerFactory factory ) : base( FactoryParameterCoercer<Parameter>.Instance )
		{
			this.configure = configure;
			this.factory = factory;
		}

		protected override IServiceLocator CreateItem( Parameter parameter )
		{
			var container = factory.Create( parameter );
			var result = new Activation.IoC.ServiceLocator( container );
			var commandParameter = new ConfigureLocationCommand.Parameter( result, result.Container, result.Logger );
			configure.ExecuteWith( commandParameter );
			return result;
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
