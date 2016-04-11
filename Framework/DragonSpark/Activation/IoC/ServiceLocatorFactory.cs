using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition.Hosting;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider> factory;

		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> configuration ) : this( new Func<IServiceProvider>( new Composition.ServiceProviderFactory( configuration ).Create ) ) {}

		public ServiceProviderFactory( Func<IServiceProvider> factory )
		{
			this.factory = factory;
		}

		protected override IServiceProvider CreateItem()
		{
			var container = new UnityContainerFactory( factory ).Create();
			var primary = new ServiceLocator( container );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary );
			return result;
		}
	}
}