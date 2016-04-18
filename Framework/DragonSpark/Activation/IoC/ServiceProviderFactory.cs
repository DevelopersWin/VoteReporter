using DragonSpark.Setup;
using System;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider> factory;

		// public ServiceProviderFactory( [Required] Func<ContainerConfiguration> configuration ) : this( new Func<IServiceProvider>( new Composition.ServiceProviderFactory( configuration ).Create ) ) {}

		public ServiceProviderFactory( Assembly[] assemblies ) : this( new Composition.ServiceProviderFactory( assemblies ).Create ) {}

		public ServiceProviderFactory( Func<IServiceProvider> factory )
		{
			this.factory = factory;
		}

		/*public ServiceProviderFactory( Type[] types ) : base( types ) {}

		public ServiceProviderFactory( Assembly[] assemblies ) : base( assemblies ) {}

		public ServiceProviderFactory( Func<ContainerConfiguration> configuration ) : base( configuration ) {}*/

		// public ServiceProviderFactory( Func<CompositionContext> source ) : base( source ) {}

		// public ServiceProviderFactory( Func<IServiceProvider> provider ) : base( provider ) {}

		protected override IServiceProvider CreateItem()
		{
			var container = new UnityContainerFactory( factory ).Create();
			var primary = new ServiceLocator( container );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary );
			return result;
		}
	}
}