using System;
using DragonSpark.Sources;

namespace DragonSpark.Runtime.Application
{
	public static class Execution
	{
		public static IAssignableSource<ISource> Context { get; } = new FixedSource<ISource>( ExecutionContext.Default );

		readonly static Func<object> Get = Context.Delegate();

		public static object Current() => Get();
	}
}