using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public interface IExceptionHandler
	{
		ExceptionHandlingResult Handle( Exception exception );
	}

	class ExceptionHandler : IExceptionHandler
	{
		public static ExceptionHandler Instance { get; } = new ExceptionHandler();
		ExceptionHandler() {}

		public ExceptionHandlingResult Handle( Exception exception ) => new ExceptionHandlingResult( true, exception );
	}
}