using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public interface IExceptionFormatter
	{
		string Format( Exception exception );
	}
}