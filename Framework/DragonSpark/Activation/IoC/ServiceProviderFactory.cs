using DragonSpark.Setup;
using System;

namespace DragonSpark.Activation.IoC
{
	public sealed class ServiceProviderFactory : FactoryBase<IServiceProvider, IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Create( IServiceProvider parameter )
		{
			var primary = new ServiceLocator( UnityContainerFactory.Instance.Create() );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), new RecursionAwareServiceProvider( primary ), parameter );
			return result;
		}
	}
}