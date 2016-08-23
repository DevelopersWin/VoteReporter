using System.Collections.Generic;
using PostSharp.Patterns.Contracts;
using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public class ReadFromKeyValuePairsCommand : ReadFromCommandBase
	{
		public ReadFromKeyValuePairsCommand() : this( new Dictionary<string, string>() ) {}

		public ReadFromKeyValuePairsCommand( IDictionary<string, string> dictionary )
		{
			Dictionary = dictionary;
		}

		[Required]
		public IDictionary<string, string> Dictionary { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.KeyValuePairs( Dictionary );
	}
}