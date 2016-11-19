using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Extensibility;
using Serilog.Events;

namespace DragonSpark.Aspects.Diagnostics
{
	sealed class LevelMappings : DictionaryCache<LogEventLevel, SeverityType>
	{
		public static LevelMappings Default { get; } = new LevelMappings();
		LevelMappings() : base(
			new Dictionary<LogEventLevel, SeverityType>
			{
				{ LogEventLevel.Verbose, SeverityType.Info },
				{ LogEventLevel.Debug, SeverityType.ImportantInfo },
				{ LogEventLevel.Information, SeverityType.ImportantInfo },
				{ LogEventLevel.Warning, SeverityType.Warning },
				{ LogEventLevel.Error, SeverityType.Error },
				{ LogEventLevel.Fatal, SeverityType.Fatal },
			}.ToImmutableDictionary() 
		) {}
	}
}