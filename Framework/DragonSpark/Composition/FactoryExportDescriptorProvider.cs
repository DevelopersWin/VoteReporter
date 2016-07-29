using DragonSpark.Activation;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Composition
{
	public sealed class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<ActivatorParameter, ISource> DefaultResult = ActivatorDelegateFactory.Instance.ToDelegate();
		public FactoryExportDescriptorProvider() : base( DefaultResult ) {}
	}
}