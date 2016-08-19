using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics.Logging
{
	public abstract class AddSinkCommand : LoggerConfigurationCommandBase<LoggerSinkConfiguration>
	{
		protected AddSinkCommand() : this( LogEventLevel.Verbose ) {}

		protected AddSinkCommand( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}

	public class AddSeqSinkCommand : AddSinkCommand
	{
		public AddSeqSinkCommand() : this( LogEventLevel.Verbose, 1000, null, null, null, null ) {}

		public AddSeqSinkCommand( LogEventLevel restrictedToMinimumLevel, int batchPostingLimit, TimeSpan? period, string apiKey, string bufferBaseFilename, long? bufferFileSizeLimitBytes ) : base( restrictedToMinimumLevel )
		{
			BatchPostingLimit = batchPostingLimit;
			Period = period;
			ApiKey = apiKey;
			BufferBaseFilename = bufferBaseFilename;
			BufferFileSizeLimitBytes = bufferFileSizeLimitBytes;
		}

		public int BatchPostingLimit { get; set; }
		public TimeSpan? Period { get; set; }
		public string ApiKey { get; set; }
		public string BufferBaseFilename { get; set; }
		public long? BufferFileSizeLimitBytes { get; set; }

		[Required]
		public Uri Endpoint { [return: Required]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration )
			=> configuration.Seq( Endpoint.ToString(), RestrictedToMinimumLevel, BatchPostingLimit, Period, ApiKey, BufferBaseFilename, BufferFileSizeLimitBytes );
	}
}