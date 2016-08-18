using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public struct ExceptionHandlingResult
	{
		public ExceptionHandlingResult( bool rethrowRecommended, Exception exception )
		{
			RethrowRecommended = rethrowRecommended;
			Exception = exception;
		}

		public bool RethrowRecommended { get; }

		public Exception Exception { get; }
	}
}