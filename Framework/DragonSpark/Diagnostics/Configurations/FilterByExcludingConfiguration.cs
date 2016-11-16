using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public class FilterByExcludingConfiguration : FilterBySpecificationConfigurationBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByExcluding( Specification.IsSatisfiedBy );
	}
}