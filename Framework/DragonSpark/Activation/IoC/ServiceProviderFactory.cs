using DragonSpark.Setup;
using System;
using System.Composition;

namespace DragonSpark.Activation.IoC
{
	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var primary = new ServiceLocator( UnityContainerFactory.Instance.Create() );
			RegisterServiceProviderCommand.Instance.Execute( primary );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary, parameter );
			return result;
		}
	}
}