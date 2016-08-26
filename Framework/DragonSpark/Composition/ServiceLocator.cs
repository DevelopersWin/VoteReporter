using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using DragonSpark.Extensions;

namespace DragonSpark.Composition
{
	public sealed class ServiceLocator : IServiceProvider
	{
		readonly CompositionContext host;

		public ServiceLocator( CompositionContext host )
		{
			this.host = host;
		}

		public object GetService( Type serviceType )
		{
			var enumerable = serviceType.GetTypeInfo().IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
			var result = enumerable ? host.GetExports( serviceType.Adapt().GetEnumerableType() ) : host.TryGet<object>( serviceType );
			return result;
		}
	}
}