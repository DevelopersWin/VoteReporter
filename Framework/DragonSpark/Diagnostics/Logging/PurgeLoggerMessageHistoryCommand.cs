using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Sources.Parameterized;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Diagnostics.Logging
{
	[ApplyAutoValidation]
	public class PurgeLoggerMessageHistoryCommand : PurgeLoggerHistoryCommand<string>
	{
		readonly static Func<IEnumerable<LogEvent>, ImmutableArray<string>> MessageFactory = LogEventMessageFactory.Default.ToSourceDelegate();

		public static PurgeLoggerMessageHistoryCommand Default { get; } = new PurgeLoggerMessageHistoryCommand();
		PurgeLoggerMessageHistoryCommand() : this( LoggingHistory.Default.Get ) {}

		// public static ISource<ICommand<Action<string>>> Defaults { get; } = new Scope<ICommand<Action<string>>>( Factory.Global( () => new PurgeLoggerMessageHistoryCommand( LoggingHistory.Default.Get() ) ) );
		public PurgeLoggerMessageHistoryCommand( Func<ILoggerHistory> historySource ) : base( historySource, MessageFactory ) {}
	}

	/*public static class MigrationProperties
	{
		public static ICache<LogEvent, bool> IsMigrating { get; } = new SourceCache<LogEvent, bool>();
	}*/

	/*public class PurgeLoggerHistoryCommand : PurgeLoggerHistoryCommand<LogEvent>
	{
		public PurgeLoggerHistoryCommand( ILoggerHistory history ) : base( history, events => events.Fixed() ) {}

		public override void Execute( Action<LogEvent> parameter ) => base.Execute( new Migrater( parameter ).Execute );

		class Migrater
		{
			readonly Action<LogEvent> action;

			public Migrater( Action<LogEvent> action )
			{
				this.action = action;
			}

			public void Execute( LogEvent parameter )
			{
				using ( MigrationProperties.IsMigrating.Assignment( parameter, true ) )
				{
					action( parameter );
				}
			}
		}
	}*/
}
