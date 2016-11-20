using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Runtime.Data;
using JetBrains.Annotations;
using Serilog.Events;
using System.Runtime.Serialization;

namespace DragonSpark.Aspects.Diagnostics
{
	[DataContract( Namespace = Defaults.Namespace )]
	public sealed class DiagnosticsConfiguration
	{
		[DataMember]
		public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

		[UsedImplicitly, DataMember]
		public TypeCollection KnownApplicationTypes { get; set; } = new TypeCollection();

		[UsedImplicitly, DataMember]
		public DtoCollection<ILoggingConfiguration> Configurations { get; set; } = new DtoCollection<ILoggingConfiguration>();
	}
}