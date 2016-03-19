using DragonSpark.Composition;
using DragonSpark.Testing.Framework.Setup;
using ServiceLocatorFactory = DragonSpark.Activation.IoC.ServiceLocatorFactory;

namespace DragonSpark.Testing.Objects.IoC
{
	public class Application : ApplicationBase
	{
		public Application( AutoData autoData ) : base( autoData, ServiceProviderFactory.Instance.Create() ) {}
	}

	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( Framework.Setup.AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : base( autoData => new Application( autoData ) ) {}
	}
}
