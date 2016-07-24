using DragonSpark.Activation;
using DragonSpark.Setup;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Composition;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var context = CompositionHostFactory.Instance.Create();
			var primary = new ServiceLocator( context );
			RegisterServiceProviderCommand.Instance.Execute( primary );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, parameter );
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
