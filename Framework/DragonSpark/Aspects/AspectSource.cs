using DragonSpark.Aspects.Build;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	class AspectSource<T> : IAspectSource where T : IAspect
	{
		readonly Func<Type, MethodInfo> methodSource;
		readonly Func<MethodInfo, AspectInstance> inner;

		public AspectSource( IMethodLocator locator ) : this( locator.Get, AspectInstance<T>.Default.Get ) {}

		public AspectSource( Func<Type, MethodInfo> methodSource, Func<MethodInfo, AspectInstance> inner )
		{
			this.methodSource = methodSource;
			this.inner = inner;
		}

		public AspectInstance Get( Type parameter )
		{
			try
			{
				var method = methodSource( parameter );
				var result = method != null ? inner( method ) : null;
				return result;
			}
			catch ( Exception )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {parameter} - {typeof(T)}", null, null, null ));
				throw;
			}
		}
	}
}