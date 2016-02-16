using System;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public class TryContext
	{
		readonly ILogger messageLogger;

		public TryContext() : this( MessageLogger.Create() ) {} // TODO: Logger Instance

		public TryContext( [Required]ILogger messageLogger )
		{
			this.messageLogger = messageLogger;
		}

		public Exception Try( Action action )
		{
			try
			{
				action();
			}
			catch ( Exception exception )
			{
				messageLogger.Debug( exception, "An exception has occurred while executing an application delegate." );
				return exception;
			}
			return null;
		}
	}
}