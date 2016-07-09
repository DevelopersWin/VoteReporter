using DragonSpark.Activation;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider( Activation.FactoryTypeLocator locator ) : base( locator, ActivatorResultFactory.Instance.ToDelegate() ) {}
	}
}