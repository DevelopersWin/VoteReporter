using JetBrains.Annotations;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Diagnostics.Configurations
{
	public class AddSeqSinkConfiguration : AddSinkConfigurationBase
	{
		public AddSeqSinkConfiguration() : this( LogEventLevel.Verbose, 1000, null, null, null, null ) {}

		[UsedImplicitly]
		public AddSeqSinkConfiguration( LogEventLevel restrictedToMinimumLevel, int batchPostingLimit, TimeSpan? period, [Optional]string apiKey, [Optional]string bufferBaseFileName, long? bufferFileSizeLimitBytes ) : base( restrictedToMinimumLevel )
		{
			BatchPostingLimit = batchPostingLimit;
			Period = period;
			ApiKey = apiKey;
			BufferBaseFileName = bufferBaseFileName;
			BufferFileSizeLimitBytes = bufferFileSizeLimitBytes;
		}

		public int BatchPostingLimit { get; set; }
		public TimeSpan? Period { get; set; }
		public string ApiKey { get; set; }
		public string BufferBaseFileName { get; set; }
		public long? BufferFileSizeLimitBytes { get; set; }

		[PostSharp.Patterns.Contracts.NotNull]
		public Uri Endpoint { [return: PostSharp.Patterns.Contracts.NotNull]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => 
			configuration.Seq( Endpoint.ToString(), RestrictedToMinimumLevel, BatchPostingLimit, Period, ApiKey, BufferBaseFileName, BufferFileSizeLimitBytes );
	}
}