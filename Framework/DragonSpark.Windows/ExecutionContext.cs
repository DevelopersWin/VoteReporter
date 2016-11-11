using DragonSpark.Sources;
using System;

namespace DragonSpark.Windows
{
	[Priority( Priority.AfterNormal )]
	sealed class ExecutionContext : Source<AppDomain>
	{
		public static ExecutionContext Default { get; } = new ExecutionContext();
		ExecutionContext() : base( AppDomain.CurrentDomain ) {}
	}
}