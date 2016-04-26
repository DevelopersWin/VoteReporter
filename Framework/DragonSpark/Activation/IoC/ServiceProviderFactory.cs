using DragonSpark.Extensions;
using DragonSpark.Setup;
using System;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider> factory;

		public ServiceProviderFactory( Assembly[] assemblies ) : this( new Composition.ServiceProviderFactory( assemblies ).Create ) {}

		public ServiceProviderFactory( Func<IServiceProvider> factory )
		{
			this.factory = factory;
		}

		protected override IServiceProvider CreateItem()
		{
			var container = new UnityContainerFactory( factory ).Create();
			var primary = new ServiceLocator( container );
			var secondary = primary.Get<IServiceProvider>();
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), new RecursionAwareServiceProvider( primary ), secondary );
			return result;
		}
	}
}