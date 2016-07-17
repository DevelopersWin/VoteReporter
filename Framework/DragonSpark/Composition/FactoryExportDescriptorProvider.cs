using DragonSpark.Activation;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider() : base( FactoryTypes.Instance.Get(), ActivatorResultFactory.Instance.ToDelegate() ) {}
	}
}