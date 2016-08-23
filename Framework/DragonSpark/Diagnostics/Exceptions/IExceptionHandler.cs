using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public interface IExceptionHandler
	{
		ExceptionHandlingResult Handle( Exception exception );
	}
}