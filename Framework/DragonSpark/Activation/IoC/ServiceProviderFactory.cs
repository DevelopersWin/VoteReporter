using DragonSpark.Setup;
using System;

namespace DragonSpark.Activation.IoC
{
	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Create( IServiceProvider parameter )
		{
			var primary = new ServiceLocator( UnityContainerFactory.Instance.Create() );
			RegisterServiceProviderCommand.Instance.Execute( primary );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary, parameter );
			return result;
		}
	}
}