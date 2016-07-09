using DragonSpark.Extensions;
using DragonSpark.Setup;
using System;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class AssemblyBasedServiceProviderFactory : ServiceProviderFactory
	{
		public AssemblyBasedServiceProviderFactory( Assembly[] assemblies ) : base( new Composition.AssemblyBasedServiceProviderFactory( assemblies ).Create ) {}
	}

	public class TypeBasedServiceProviderFactory : ServiceProviderFactory
	{
		public TypeBasedServiceProviderFactory( Type[] types ) : base( new Composition.TypeBasedServiceProviderFactory( types ).Create ) {}
	}

	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider> factory;

		public ServiceProviderFactory( Func<IServiceProvider> factory )
		{
			this.factory = factory;
		}

		public override IServiceProvider Create()
		{
			var provider = factory();
			var containerFactory = new UnityContainerFactory( provider );
			var container = containerFactory.Create();
			var primary = new ServiceLocator( container );
			var secondary = primary.Get<IServiceProvider>();
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary.ToItem() ), new RecursionAwareServiceProvider( primary ), secondary );
			return result;
		}
	}
}