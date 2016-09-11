using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Extensibility
{
	public struct AspectInvocation : IInvocation
	{
		readonly Func<object> proceed;

		public AspectInvocation( Arguments arguments, Func<object> proceed )
		{
			Arguments = arguments;
			this.proceed = proceed;
		}

		public Arguments Arguments { get; }

		public object Invoke( object parameter )
		{
			Arguments.SetArgument( 0, parameter );
			var result = proceed();
			return result;
		}
	}
}