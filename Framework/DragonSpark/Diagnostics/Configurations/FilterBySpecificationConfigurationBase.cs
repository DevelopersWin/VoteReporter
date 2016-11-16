using DragonSpark.Specifications;
using PostSharp.Patterns.Contracts;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class FilterBySpecificationConfigurationBase : FilterConfigurationBase
	{
		[Required]
		public ISpecification<LogEvent> Specification { [return: Required]get; set; }
	}
}