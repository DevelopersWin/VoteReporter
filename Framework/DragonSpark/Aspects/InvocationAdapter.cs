using System;

namespace DragonSpark.Aspects
{
	public sealed class InvocationAdapter : IInvocation
	{
		readonly Func<object> factory;

		public InvocationAdapter( Func<object> factory )
		{
			this.factory = factory;
		}

		public object Get( object parameter ) => factory();
	}
}