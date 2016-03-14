using DragonSpark.Activation.FactoryModel;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider( DiscoverableFactoryTypeLocator locator ) : base( locator, ActivatorFactory.Instance ) {}
	}
}