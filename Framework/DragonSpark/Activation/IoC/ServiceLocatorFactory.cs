using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition.Hosting;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderCoreFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider> factory;

		public ServiceProviderCoreFactory( [Required] Func<ContainerConfiguration> configuration ) : this( new Func<IServiceProvider>( new Composition.ServiceProviderCoreFactory( configuration ).Create ) ) {}

		public ServiceProviderCoreFactory( Func<IServiceProvider> factory )
		{
			this.factory = factory;
		}

		protected override IServiceProvider CreateItem()
		{
			var source = new UnityContainerFactory( factory ).Create();
			var primary = new ServiceLocator( source );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary );
			return result;
		}
	}
}