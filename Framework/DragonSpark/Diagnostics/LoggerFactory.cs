using Serilog;
using Serilog.Core;

namespace DragonSpark.Diagnostics
{
	public static class LoggerExtensions
	{
		public static ILogger ForSource( this ILogger @this, object context )
		{
			var formatted = FormatterFactory.Instance.From( context );
			var result = @this.ForContext( Constants.SourceContextPropertyName, formatted, true );
			return result;
		}
	}


	/*public class LoggerFactory : FactoryBase<ILogger>
	{
		readonly Func<LoggerConfiguration> configurationSource;
		readonly Func<object> contextSource;

		public LoggerFactory( [Required] Func<LoggerConfiguration> configurationSource ) : this( configurationSource, Defaults.ExecutionContext ) {}

		public LoggerFactory( [Required] Func<LoggerConfiguration> configurationSource, Func<object> contextSource )
		{
			this.configurationSource = configurationSource;
			this.contextSource = contextSource;
		}

		public override ILogger Create()
		{
			var context = contextSource();
			var loggerConfiguration = configurationSource();
			var logger = loggerConfiguration.CreateLogger();
			var forContext = logger.ForSource( context );
			return forContext;
		}
	}*/
}