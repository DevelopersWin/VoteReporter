using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging
{
	sealed class ApplicationAssemblyTransform : TransformerBase<LoggerConfiguration>, ILogEventEnricher
	{
		public static ApplicationAssemblyTransform Default { get; } = new ApplicationAssemblyTransform();
		ApplicationAssemblyTransform() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.Enrich.With( this );

		public void Enrich( LogEvent logEvent, ILogEventPropertyFactory propertyFactory ) => logEvent.AddPropertyIfAbsent( propertyFactory.CreateProperty( nameof(AssemblyInformation), DefaultAssemblyInformationSource.Default.Get(), true ) );
	}
}