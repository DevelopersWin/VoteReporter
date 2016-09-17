using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	sealed class AspectInstance<T> : IAspectInstance where T : IAspect
	{
		readonly Func<Type, MethodInfo> methodSource;
		readonly Func<MethodInfo, AspectInstance> inner;

		public AspectInstance( IMethodLocator locator ) : this( locator.Get, AspectInstanceFactory<T>.Default.Get ) {}

		public AspectInstance( Func<Type, MethodInfo> methodSource, Func<MethodInfo, AspectInstance> inner )
		{
			this.methodSource = methodSource;
			this.inner = inner;
		}

		public AspectInstance Get( Type parameter )
		{
			var method = methodSource( parameter );
			var result = method != null ? inner( method ) : null;
			return result;
		}
	}
}