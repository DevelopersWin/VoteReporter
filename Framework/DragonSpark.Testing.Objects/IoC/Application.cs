using DragonSpark.Composition;

namespace DragonSpark.Testing.Objects.IoC
{
	/*public class Application : ApplicationBase
	{
		public Application() : base( ServiceProviderFactory.Instance.Create() ) {}
	}*/

	public class ServiceProviderFactory : Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( Framework.Setup.AssemblyProvider.Instance.Create() ).Create ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : base( autoData => ServiceProviderFactory.Instance.Create() ) {}
	}
}
