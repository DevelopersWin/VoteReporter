using System.Windows.Markup;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog.Configuration;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	[ContentProperty( nameof(Items) )]
	public class EnrichCommand : EnrichCommandBase
	{
		public DeclarativeCollection<ILogEventEnricher> Items { get; } = new DeclarativeCollection<ILogEventEnricher>();
		
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.With( EnumerableExtensions.Fixed( Items ) );
	}
}