using DragonSpark.Commands;
using DragonSpark.Extensions;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics
{
	public abstract class PurgeLoggerHistoryCommandBase<T> : CommandBase<Action<T>>
	{
		readonly ILoggerHistory history;
		readonly Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory;

		protected PurgeLoggerHistoryCommandBase( ILoggerHistory history, Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory )
		{
			this.history = history;
			this.factory = factory;
		}

		public override void Execute( Action<T> parameter )
		{
			factory( history.Events ).Each( parameter );
			history.Clear();
		}
	}
}