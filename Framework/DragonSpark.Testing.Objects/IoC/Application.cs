using DragonSpark.Composition;
using DragonSpark.Testing.Framework.Setup;
using ServiceLocatorFactory = DragonSpark.Activation.IoC.ServiceLocatorFactory;

namespace DragonSpark.Testing.Objects.IoC
{
	public class Application : ApplicationBase
	{
		public Application() : base( ApplicationServiceProviderFactory.Instance.Create() ) {}
	}

	public class ApplicationServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ApplicationServiceProviderFactory Instance { get; } = new ApplicationServiceProviderFactory();

		public ApplicationServiceProviderFactory() : base( Framework.Setup.AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : base( () => new Application() ) {}
	}
}
