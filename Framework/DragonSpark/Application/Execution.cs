using System;
using DragonSpark.Sources;

namespace DragonSpark.Application
{
	public static class Execution
	{
		public static IAssignableSource<ISource> Context { get; } = new SuppliedSource<ISource>( ExecutionContext.Default );

		readonly static Func<object> Get = Context.Delegate();

		public static object Current() => Get();
	}
}