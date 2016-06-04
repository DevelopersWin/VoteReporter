﻿using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using System;
using Serilog.Events;

namespace DragonSpark.Diagnostics
{
	public static class LoggerExtensions
	{
		public static ILogger ForSource( this ILogger @this, object context )
		{
			var factory = Services.Get<FormatterFactory>();
			var formatted = factory.From( context );
			var result = @this.ForContext( Constants.SourceContextPropertyName, formatted, true );
			return result;
		}
	}


	public class LoggerFactory : FactoryBase<ILogger>
	{
		
		readonly Func<LoggerConfiguration> configurationSource;
		readonly Func<object> contextSource;

		public LoggerFactory( [Required] Func<LoggerConfiguration> configurationSource ) : this( configurationSource, Execution.GetCurrent ) {}

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
	}
}