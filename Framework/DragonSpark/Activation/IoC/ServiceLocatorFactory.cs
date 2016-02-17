using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System.Reflection;
using System.Windows.Markup;
using DragonSpark.TypeSystem;

namespace DragonSpark.Activation.IoC
{
	public abstract class UnityContainerFactory<TAssemblyProvider, TLogger> : UnityContainerFactory<TAssemblyProvider>
		where TAssemblyProvider : IAssemblyProvider, new()
		where TLogger : IFactory<ILogger>, new()
	{
		protected UnityContainerFactory() : base( new TLogger().Create() ) {}
	}

	public abstract class UnityContainerFactory<TAssemblyProvider> : UnityContainerFactory
		where TAssemblyProvider : IAssemblyProvider, new()
	{
		protected UnityContainerFactory( ILogger logger ) : this( new TAssemblyProvider().Create(), logger ) {}

		protected UnityContainerFactory( [Required]Assembly[] assemblies, [Required]ILogger logger )
		{
			Assemblies = assemblies;
			Logger = logger;
		}
	}

	public class AssignAssembliesCommand : Command<object>
	{
		[Required]
		public Assembly[] Assemblies { [return: Required]get; set; }

		protected override void OnExecute( object parameter ) => new AssemblyHost().Assign( Assemblies );
	}

	public class UnityContainerFactory : FactoryBase<IUnityContainer>
	{
		[Required]
		public Assembly[] Assemblies { get; set; }

		[Required]
		public ILogger Logger { get; set; }

		protected override IUnityContainer CreateItem()
		{
			var assemblies = Assemblies ?? TypeSystem.Assemblies.GetCurrent();
			var logger = Logger ?? Factory.Create<ILogger>();
			var result = new UnityContainer()
				.RegisterInstance( assemblies )
				.RegisterInstance( logger )
				.Extend<RegistrationMonitorExtension>()
				.Extend<BuildPipelineExtension>();
			return result;
		}
	}

	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		[Freeze]
		protected override void Dispose( bool disposing ) => base.Dispose( disposing );
	}

	[ContentProperty( nameof(Container) )]
	public class ConfigureLocationCommand : Command<object>
	{
		public class Context
		{
			public Context( [Required]IUnityContainer container ) : this( container, container.Resolve<ILogger>() ) {}

			public Context( [Required]IUnityContainer container, [Required]ILogger logger ) : this( Services.Location, container, logger ) {}

			public Context( [Required]IServiceLocation location, [Required]IUnityContainer container, [Required]ILogger logger ) : this( location, new ServiceLocator( container ), container, logger ) {}

			public Context( [Required]IServiceLocation location, [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger )
			{
				Location = location;
				Locator = locator;
				Container = container;
				Logger = logger;
			}

			public IUnityContainer Container { get; }

			public IServiceLocation Location { get; }

			public IServiceLocator Locator { get; }

			public ILogger Logger { get; }
		}

		[Required]
		public IUnityContainer Container { get; set; }

		protected override void OnExecute( object parameter )
		{
			var container = Container ?? Factory.Create<IUnityContainer>();
			var context = new Context( container );
			Configure( context );
		}

		protected virtual void Configure( Context context )
		{
			context.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			context.Container.RegisterInstance( context.Location );
			context.Container.RegisterInstance( context.Locator, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}

	public class AssignLocationCommand<T> : AssignLocationCommand where T : UnityContainerFactory, new()
	{
		public AssignLocationCommand() : this( new T().Create() ) {}

		public AssignLocationCommand( IUnityContainer container )
		{
			Container = container;
		}

	}

	public class AssignLocationCommand : ConfigureLocationCommand
	{
		protected override void Configure( Context context )
		{
			context.Location.Assign( context.Locator );
			base.Configure( context );
		}
	}
}