using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Runtime.Data;
using JetBrains.Annotations;
using Serilog.Events;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class DiagnosticsConfiguration
	{
		public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

		[UsedImplicitly]
		public TypeCollection KnownApplicationTypes { get; set; } = new TypeCollection();

		[UsedImplicitly]
		public DtoCollection<ILoggingConfiguration> Configurations { get; set; } = new DtoCollection<ILoggingConfiguration>();
	}
}