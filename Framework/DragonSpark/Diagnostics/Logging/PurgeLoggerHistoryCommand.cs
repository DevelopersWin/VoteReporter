using DragonSpark.Aspects.Extensibility;
using DragonSpark.Extensions;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics.Logging
{
	public abstract class PurgeLoggerHistoryCommand<T> : ExtensibleCommandBase<Action<T>>
	{
		readonly Func<ILoggerHistory> historySource;
		readonly Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory;

		protected PurgeLoggerHistoryCommand( Func<ILoggerHistory> historySource, Func<IEnumerable<LogEvent>, ImmutableArray<T>> factory )
		{
			this.historySource = historySource;
			this.factory = factory;
		}

		public override void Execute( Action<T> parameter )
		{
			var history = historySource();
			factory( history.Events ).Each( parameter );
			history.Clear();
		}
	}
}