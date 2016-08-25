using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	sealed class ApplicationAssemblyTransform : TransformerBase<LoggerConfiguration>
	{
		public static ApplicationAssemblyTransform Default { get; } = new ApplicationAssemblyTransform();
		ApplicationAssemblyTransform() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			var information = DefaultAssemblyInformationSource.Default.Get();
			var result = information != null ? parameter.Enrich.With( new Enricher( information ) ) : parameter;
			return result;
		}

		sealed class Enricher : ILogEventEnricher
		{
			readonly AssemblyInformation information;
			public Enricher( AssemblyInformation information )
			{
				this.information = information;
			}

			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( nameof(AssemblyInformation), information, true ) );
		}
	}
}