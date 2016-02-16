using System;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public class TryContext
	{
		readonly ILogger logger;

		public TryContext() : this( new RecordingSinkFactory().Create() ) {} // TODO: Logger Instance

		public TryContext( [Required]ILogger logger )
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