using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Diagnostics.Logging
{
	public interface ILoggerHistory : ILogEventSink
	{
		IEnumerable<LogEvent> Events { get; }

		void Clear();
	}
	
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

	public abstract class PurgeLoggerHistoryCommand<T> : CommandBase<Action<T>>
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

	public class MethodFormatter : IFormattable
	{
		readonly MethodBase method;

		public MethodFormatter( MethodBase method )
		{
			this.method = method;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => $"{method.DeclaringType.Name}.{method.Name}";
	}
}
