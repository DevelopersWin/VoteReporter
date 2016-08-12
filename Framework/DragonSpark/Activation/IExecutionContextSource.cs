using DragonSpark.Runtime;
using System;
using DragonSpark.Runtime.Sources;

namespace DragonSpark.Activation
{
	public static class Execution
	{
		public static IAssignableSource<ISource> Context { get; } = new FixedSource<ISource>( ExecutionContext.Instance );

		readonly static Func<object> Get = Context.Delegate();

		public static object Current() => Get();
	}

	[Priority( Priority.Low )]
	class ExecutionContext : StoreBase<object>
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		ExecutionContext() {}

		protected override object Get() => this;
	}
}