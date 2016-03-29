using DragonSpark.Activation;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider( FactoryTypeRequestLocator locator ) : base( locator, ActivatorFactory.Instance ) {}
	}
}