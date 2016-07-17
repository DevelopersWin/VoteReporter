using DragonSpark.Activation;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Composition;
using Type = System.Type;

namespace DragonSpark.Composition
{
	/*public class TypeBasedServiceProviderFactory : ServiceProviderFactory
	{
		public TypeBasedServiceProviderFactory( IEnumerable<Type> types ) : base( new TypeBasedConfigurationContainerFactory( types ) ) {}
	}*/

	/*public class AssemblyBasedServiceProviderFactory : ServiceProviderFactoryBase<IEnumerable<Assembly>>
	{
		public static AssemblyBasedServiceProviderFactory Instance { get; } = new AssemblyBasedServiceProviderFactory();
		AssemblyBasedServiceProviderFactory() {}

		public override IServiceProvider Create( IEnumerable<Assembly> parameter )
		{
			var assemblies = parameter.Fixed();
			var result = base.Create( assemblies );
			return result;
		}
	}*/

	public sealed class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Create()
		{
			var context = CompositionSource.Instance.Get();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), new RecursionAwareServiceProvider( primary ), DefaultServiceProvider.Instance );
			return result;
		}
	}

	public sealed class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionContext host;

		public ServiceLocator( CompositionContext host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => host.TryGet<object>( serviceType, key );
	}
}
