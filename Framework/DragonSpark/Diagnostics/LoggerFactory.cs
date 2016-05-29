using DragonSpark.Activation;
using DragonSpark.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using System;

namespace DragonSpark.Diagnostics
{
	public static class LoggerExtensions
	{
		public static ILogger ForSource( this ILogger @this, [Formatted]object context ) => @this.ForContext( Constants.SourceContextPropertyName, context, true );
	}


	public class LoggerFactory : FactoryBase<ILogger>
	{
		readonly Func<LoggerConfiguration> configurationSource;
		readonly object context;

		public LoggerFactory( [Required] Func<LoggerConfiguration> configurationSource ) : this( configurationSource, Execution.GetCurrent() ) {}

		public LoggerFactory( [Required] Func<LoggerConfiguration> configurationSource, object context )
		{
			this.configurationSource = configurationSource;
			this.context = context;
		}

		public override ILogger Create()
		{
			var loggerConfiguration = configurationSource();
			var logger = loggerConfiguration.CreateLogger();
			var forContext = logger.ForSource( context );
			return forContext;
		}
	}
}