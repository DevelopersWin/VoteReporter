using JetBrains.Annotations;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics.Configurations
{
	public class AddSinkConfiguration : AddSinkConfigurationBase
	{
		public AddSinkConfiguration() {}

		public AddSinkConfiguration( ILogEventSink sink )
		{
			Sink = sink;
		}

		[UsedImplicitly, PostSharp.Patterns.Contracts.NotNull]
		public ILogEventSink Sink { [return: PostSharp.Patterns.Contracts.NotNull]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.Sink( Sink );
	}

	public abstract class AddSinkConfigurationBase : LoggingConfigurationBase<LoggerSinkConfiguration>
	{
		protected AddSinkConfigurationBase() : this( LogEventLevel.Verbose ) {}

		protected AddSinkConfigurationBase( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}
}