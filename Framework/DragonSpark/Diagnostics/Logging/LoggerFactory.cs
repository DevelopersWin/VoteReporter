using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging
{
	public static class LoggerExtensions
	{
		public static ILogger ForSource( this ILogger @this, object context ) => 
			@this.ForContext( Constants.SourceContextPropertyName, Formatter.Instance.Format( context ), true );
	}
}