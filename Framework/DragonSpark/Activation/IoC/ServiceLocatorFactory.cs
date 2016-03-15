using DragonSpark.Aspects;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Activation.IoC
{
	/*public class UnityContainerFactory : FactoryBase<IUnityContainer>
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
	}*/

	public class FrozenDisposeContainerControlledLifetimeManager : ContainerControlledLifetimeManager
	{
		[Freeze]
		protected override void Dispose( bool disposing ) => base.Dispose( disposing );
	}

	// [Export]
	
	// [ContentProperty( nameof(Parameter) )]
	public class ConfigureLocationCommand : Command<ConfigureLocationCommand.Parameter>
	{
		public static ConfigureLocationCommand Instance { get; } = new ConfigureLocationCommand();

		public class Parameter
		{
			public Parameter( [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger ) : this( Services.Location, locator, container, logger ) {}

			public Parameter( [Required]IServiceLocation location, [Required]IServiceLocator locator, [Required]IUnityContainer container, [Required]ILogger logger )
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
		
		protected override void OnExecute( Parameter parameter )
		{
			parameter.Logger.Information( Resources.ConfiguringServiceLocatorSingleton );
			parameter.Container.RegisterInstance( parameter.Location );
			parameter.Container.RegisterInstance( parameter.Locator, new FrozenDisposeContainerControlledLifetimeManager() );
		}
	}

	public class AssignLocationCommand : ConfigureLocationCommand
	{
		public new static AssignLocationCommand Instance { get; } = new AssignLocationCommand();

		protected override void OnExecute( Parameter parameter )
		{
			parameter.Location.Assign( parameter.Locator );
			base.OnExecute( parameter );
		}
	}
}