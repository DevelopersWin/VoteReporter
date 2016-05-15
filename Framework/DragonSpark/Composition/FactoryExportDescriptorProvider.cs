using DragonSpark.Activation;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider( FactoryTypeLocator locator ) : base( locator, ActivatorFactory.Instance ) {}
	}
}