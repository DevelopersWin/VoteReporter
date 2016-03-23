using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	[Persistent]
	public class TryContext
	{
		readonly Func<ILogger> logger;

		public TryContext( [Required]Func<ILogger> logger )
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
				logger().Debug( exception, "An exception has occurred while executing an application delegate." );
				return exception;
			}
			return null;
		}
	}
}