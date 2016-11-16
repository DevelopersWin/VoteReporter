using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public class FilterByIncludingOnlyConfiguration : FilterBySpecificationConfigurationBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByIncludingOnly( Specification.IsSatisfiedBy );
	}
}