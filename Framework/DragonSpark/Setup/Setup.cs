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
using ServiceLocator = DragonSpark.Activation.IoC.ServiceLocator;
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

	public interface IApplicationContext : IServiceProvider
	{
		Assembly[] Assemblies { get; }
	}

	public class ApplicationContextFactory : FactoryBase<IApplicationContext>
	{
		readonly Func<Assembly[]> assemblies;
		readonly Func<Assembly[], CompositionHost> compositionHostFactory;
		readonly Func<ServiceLocatorFactory.Parameter, IServiceLocator> serviceLocatorFactory;

		public ApplicationContextFactory( [Required]IAssemblyProvider assemblyProvider, [Required]CompositionHostFactory compositionHostFactory, [Required]ServiceLocatorFactory serviceLocatorFactory ) : this( assemblyProvider.Create, compositionHostFactory.Create, serviceLocatorFactory.Create ) {}

		public ApplicationContextFactory( [Required]Func<Assembly[]> assemblies, [Required]Func<Assembly[], CompositionHost> compositionHostFactory, [Required]Func<ServiceLocatorFactory.Parameter, IServiceLocator> serviceLocatorFactory )
		{
			this.assemblies = assemblies;
			this.compositionHostFactory = compositionHostFactory;
			this.serviceLocatorFactory = serviceLocatorFactory;
		}

		protected override IApplicationContext CreateItem()
		{
			var instance = assemblies();
			var host = compositionHostFactory( instance );
			var parameter = new ServiceLocatorFactory.Parameter( host, instance );
			var serviceLocator = serviceLocatorFactory( parameter );
			var result = new ApplicationContext( instance, serviceLocator );
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

	public class MefServiceLocatorAdapter : ServiceLocatorImplBase
	{
		readonly CompositionHost host;

		public MefServiceLocatorAdapter(CompositionHost host)
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

		protected override IServiceLocator CreateItem( ServiceLocatorFactory.Parameter parameter ) => new MefServiceLocatorAdapter( parameter.Host );
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

		public static ServiceLocatorFactory Assigned { get; } = new ServiceLocatorFactory( AssignLocationCommand.Instance );

		public static ServiceLocatorFactory Configured { get; } = new ServiceLocatorFactory();

		readonly ConfigureLocationCommand configure;
		readonly UnityContainerFactory factory;

		public ServiceLocatorFactory() : this( ConfigureLocationCommand.Instance ) {}

		public ServiceLocatorFactory( ConfigureLocationCommand configure ) : this( configure, UnityContainerFactory.Instance ) {}

		public ServiceLocatorFactory( ConfigureLocationCommand configure, UnityContainerFactory factory )
		{
			this.configure = configure;
			this.factory = factory;
		}

		protected override IServiceLocator CreateItem( Parameter parameter )
		{
			var container = factory.Create( parameter );
			var result = new ServiceLocator( container );
			var commandParameter = new ConfigureLocationCommand.Parameter( result, result.Container, result.Logger );
			configure.ExecuteWith( commandParameter );
			return result;
		}
	}

	class ApplicationContext : IApplicationContext
	{
		readonly IServiceLocator locator;

		public ApplicationContext( [Required]Assembly[] assemblies, [Required]IServiceLocator locator )
		{
			this.locator = locator;
			Assemblies = assemblies;
		}

		public object GetService( Type serviceType ) => locator.GetInstance( serviceType );

		public Assembly[] Assemblies { get; }
	}

	public interface IApplication : ICommand, IDisposable
	{
		IApplicationContext Context { get; }
	}

	public static class ApplicationServices
	{
		public static IApplication Current => new CurrentApplication().Item;
	}

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication
	{
		protected Application( [Required]IApplicationContext context, IEnumerable<ICommand> commands ) : this( commands )
		{
			Context = context;
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
		public IApplicationContext Context { [return: Required]get; set; }

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
	}

	public class ApplyExportedCommandsCommand<T> : Command<object> where T : ICommand
	{
		[Required, ApplicationService]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		protected override void OnExecute( object parameter )
		{
			var enumerable = Host.GetExports<T>( ContractName );
			enumerable.Prioritize().Each( setup => { setup.ExecuteWith( parameter ); } );
		}
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
