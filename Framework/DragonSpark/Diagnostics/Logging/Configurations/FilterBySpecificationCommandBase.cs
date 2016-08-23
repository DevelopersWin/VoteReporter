using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using Serilog.Events;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public abstract class FilterBySpecificationCommandBase : FilterCommandBase
	{
		[Required]
		public ISpecification<LogEvent> Specification { [return: Required]get; set; }
	}
}