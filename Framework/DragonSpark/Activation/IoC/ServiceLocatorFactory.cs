using System.Composition;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System.Windows.Markup;
using DragonSpark.ComponentModel;

namespace DragonSpark.Activation.IoC
{
	public class UnityContainerFactory : FactoryBase<IUnityContainer>
	{
		protected override IUnityContainer CreateItem()
		{
			var result = new UnityContainer()
				.Extend<DefaultRegistrationsExtension>()
				.Extend<BuildPipelineExtension>()
				.Extend<InstanceTypeRegistrationMonitorExtension>()
				.Extend<CompositionExtension>()
				;
			return result;
		}
	}

	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		[Freeze]
		protected override void Dispose( bool disposing ) => base.Dispose( disposing );
	}

	[Export]
	public class ConfigureServiceLocationContext
	{
		[ImportingConstructor]
		public ConfigureServiceLocationContext( [Required]IUnityContainer container, [Required]ILogger logger ) : this( Services.Location, new ServiceLocator( container, logger ), container, logger ) {}

		public ConfigureServiceLocationContext( [Required]IServiceLocation location, [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger )
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

	[ContentProperty( nameof(Context) )]
	public class ConfigureLocationCommand : Command<object>
	{
		public ConfigureLocationCommand() {}

		public ConfigureLocationCommand( [Required]ConfigureServiceLocationContext context )
		{
			Context = context;
		}

		// public ConfigureLocationCommand() : this( Composer.Compose<IUnityContainer>(), Composer.Compose<ILogger>() ) {}

		// public ConfigureLocationCommand( [Required]IUnityContainer container, ILogger logger ) : this( new ServiceLocator( container, logger ), container, logger ) {}

		/*public ConfigureLocationCommand( [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger )
		{
			Locator = locator;
			Container = container;
			Logger = logger;
		}*/

		[Required, Compose]
		public ConfigureServiceLocationContext Context { [return: Required]get; set; }

		/*[Required, ComponentModel.Singleton( typeof(Services), nameof(Services.Location) )]
		public IServiceLocation Location { [return: Required] get; set; }

		[Required]
		public IUnityContainer Container { get; set; }

		[Required]
		public IServiceLocator Locator { get; set; }

		[Required]
		public ILogger Logger { get; set; }*/

		protected override void OnExecute( object parameter )
		{
			Context.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			Context.Container.RegisterInstance( Context.Location );
			Context.Container.RegisterInstance( Context.Locator, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}

	/*public class AssignLocationCommand<T> : AssignLocationCommand where T : UnityContainerFactory, new()
	{
		public AssignLocationCommand() : this( new T().Create() ) {}

		public AssignLocationCommand( IUnityContainer container ) : base()
		
	}*/

	public class AssignLocationCommand : ConfigureLocationCommand
	{
		protected override void OnExecute( object parameter )
		{
			Context.Location.Assign( Context.Locator );
			base.OnExecute( parameter );
		}
	}
}