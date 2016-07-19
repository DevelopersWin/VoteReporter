using DragonSpark.Activation;
using System;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<Activator.Parameter, object> DefaultResult = ActivatorResultFactory.Instance.ToDelegate();
		public FactoryExportDescriptorProvider() : base( FactoryTypes.Instance.Value, DefaultResult ) {}
	}
}