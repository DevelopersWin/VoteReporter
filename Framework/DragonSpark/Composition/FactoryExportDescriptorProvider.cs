using DragonSpark.Activation;
using System;

namespace DragonSpark.Composition
{
	public sealed class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<Activator.Parameter, object> DefaultResult = ActivatorResultFactory.Instance.ToDelegate();
		public FactoryExportDescriptorProvider() : base( DefaultResult ) {}
	}
}