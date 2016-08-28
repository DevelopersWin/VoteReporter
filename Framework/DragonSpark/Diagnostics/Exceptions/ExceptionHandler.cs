using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class ExceptionHandler : IExceptionHandler
	{
		public static ExceptionHandler Default { get; } = new ExceptionHandler();
		ExceptionHandler() {}

		public ExceptionHandlingResult Handle( Exception exception ) => new ExceptionHandlingResult( true, exception );
	}
}