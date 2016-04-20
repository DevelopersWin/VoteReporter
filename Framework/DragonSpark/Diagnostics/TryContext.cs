using DragonSpark.Setup.Registration;
using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	[Persistent]
	public class TryContext
	{
		readonly ILogger logger;

		public TryContext( ILogger logger )
		{
			this.logger = logger;
		}

		public Exception Try( Action action )
		{
			try
			{
				action();
			}
			catch ( Exception exception )
			{
				logger.Debug( exception, "An exception has occurred while executing an application delegate." );
				return exception;
			}
			return null;
		}
	}
}