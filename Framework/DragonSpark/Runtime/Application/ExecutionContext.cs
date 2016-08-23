using DragonSpark.Sources;

namespace DragonSpark.Runtime.Application
{
	[Priority( Priority.Low )]
	class ExecutionContext : SourceBase<object>
	{
		public static ExecutionContext Default { get; } = new ExecutionContext();
		ExecutionContext() {}

		public override object Get() => this;
	}
}