using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public static class Extensions
	{
		public static void Process( this IExceptionHandler target, Exception exception )
		{
			var handled = target.Handle( exception );
			if ( handled.RethrowRecommended )
			{
				throw handled.Exception;
			}
		}
	}
}