using DragonSpark.Setup.Registration;
using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	/*[Persistent]
	public class LoggerSource : FirstFactory<ILogger>
	{
		public LoggerSource( Func<ILogger> inner ) : base( inner, Services.Get<ILogger> ) {}
	}*/

	[Persistent]
	public class TryContext
	{
		readonly Func<ILogger> source;

		public TryContext( Func<ILogger> source )
		{
			this.source = source;
		}

		public Exception Try( Action action )
		{
			try
			{
				action();
			}
			catch ( Exception exception )
			{
				var logger = source();
				logger.Debug( exception, "An exception has occurred while executing an application delegate." );
				return exception;
			}
			return null;
		}
	}
}