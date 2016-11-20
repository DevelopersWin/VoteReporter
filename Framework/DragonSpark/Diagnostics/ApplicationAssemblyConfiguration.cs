using DragonSpark.Application;
using DragonSpark.Diagnostics.Configurations;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Diagnostics
{
	public sealed class ApplicationAssemblyConfiguration : LoggingConfigurationBase
	{
		public static ApplicationAssemblyConfiguration Default { get; } = new ApplicationAssemblyConfiguration();
		ApplicationAssemblyConfiguration() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			var information = CurrentApplicationInformation.Default.Get();
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