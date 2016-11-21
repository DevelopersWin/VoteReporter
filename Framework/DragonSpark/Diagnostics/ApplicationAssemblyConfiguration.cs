using DragonSpark.Application;
using DragonSpark.Diagnostics.Configurations;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Diagnostics
{
	public sealed class ApplicationAssemblyConfiguration : LoggingConfigurationBase
	{
		public static ApplicationAssemblyConfiguration Default { get; } = new ApplicationAssemblyConfiguration();
		ApplicationAssemblyConfiguration() : this( AssemblyInformationContext.Default.Get ) {}

		readonly Func<AssemblyInformation> source;

		[UsedImplicitly]
		public ApplicationAssemblyConfiguration( Func<AssemblyInformation> source )
		{
			this.source = source;
		}

		public override LoggerConfiguration Get( LoggerConfiguration parameter )
		{
			var information = source();
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

			public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => 
				logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( nameof(AssemblyInformation), information, true ) );
		}
	}

	/*public class LoggingProperty<T> : SourceBase<ILogEventEnricher>
	{
		public LoggingProperty( Func<T> source ) {}

		public override ILogEventEnricher Get()
		{
			return null;
		}
	}*/
}