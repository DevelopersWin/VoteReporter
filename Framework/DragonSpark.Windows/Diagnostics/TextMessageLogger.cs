using DragonSpark.Diagnostics;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.IO;

namespace DragonSpark.Windows.Diagnostics
{
	public class TextMessageLogger : MessageLoggerBase, IDisposable
	{
		readonly TextWriter writer;

		public TextMessageLogger() : this( Console.Out ) {}

		public TextMessageLogger( [Required]TextWriter writer )
		{
			this.writer = writer;
		}

		protected override void OnLog( Message message ) => writer.WriteLine( message.Text );

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				writer.Dispose();
			}
		}
	}

	public class AddSeqSinkCommand : AddSinkCommand
	{
		public AddSeqSinkCommand( LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, int batchPostingLimit = 1000, TimeSpan? period = null, string apiKey = null, string bufferBaseFilename = null, long? bufferFileSizeLimitBytes = null ) : base( restrictedToMinimumLevel )
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

	public class AddRollingFileSinkCommand : AddSinkCommand
	{
		public AddRollingFileSinkCommand( LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", long fileSizeLimitBytes = 1073741824, int retainedFileCountLimit = 31 ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
			FileSizeLimitBytes = fileSizeLimitBytes;
			RetainedFileCountLimit = retainedFileCountLimit;
		}

		[NotEmpty]
		public string PathFormat { [return: NotEmpty]get; set; }

		public string OutputTemplate { get; set; }

		public long FileSizeLimitBytes { get; set; }

		public int RetainedFileCountLimit { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) 
			=> configuration.RollingFile( PathFormat, RestrictedToMinimumLevel, OutputTemplate, FormatProvider, FileSizeLimitBytes, RetainedFileCountLimit );
	}
}