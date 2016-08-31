using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ServiceLocator : ActivatorBase
	{
		readonly CompositionContext host;

		public ServiceLocator( CompositionContext host )
		{
			this.host = host;
		}

		public override object Get( Type parameter )
		{
			var enumerable = parameter.GetTypeInfo().IsGenericType && parameter.GetGenericTypeDefinition() == typeof(IEnumerable<>);
			var result = enumerable ? host.GetExports( parameter.Adapt().GetEnumerableType() ) : host.TryGet<object>( parameter );
			return result;
		}
	}
}