using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DragonSpark.Diagnostics
{
	public class Diagnostics : IDiagnostics
	{
		readonly RecordingLogEventSink sink;

		public Diagnostics( [Required] ILogger logger, [Required] RecordingLogEventSink sink, [Required] LoggingLevelSwitch levelSwitch )
		{
			Logger = logger;
			this.sink = sink;
			Switch = levelSwitch;
		}

		public ILogger Logger { get;  }

		public LoggingLevelSwitch Switch { get; }

		public IEnumerable<LogEvent> Events => sink.Events;

		public void Purge( Action<string> writer ) => PurgingEventFactory.Instance.Create( sink ).Each( writer );

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose() {}
	}

	public static class ExceptionSupport
	{
		public static Exception Try( Action action ) => Try( Services.Get<TryContext>, action );

		public static Exception Try( this Func<TryContext> @this, Action action ) => @this().Try( action );

		public static void Process( this IExceptionHandler target, Exception exception ) => target.Handle( exception ).With( a => a.RethrowRecommended.IsTrue( () => { throw a.Exception; } ) );
	}

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Profiler : IProfiler
	{
		readonly ILogger logger;

		readonly Tracker tracker = new Tracker();

		public Profiler( [Required] ILogger logger )
		{
			this.logger = logger.ForContext<Profiler>();
		}

		public virtual void Start() => tracker.Initialize();

		public void Mark( string @event ) => logger.Information( "{Time}: {Event} ({Since})", tracker.Time, @event, tracker.Mark() );

		class Tracker : IDisposable
		{
			readonly Stopwatch watcher = new Stopwatch();

			readonly FixedValue<long> last = new FixedValue<long>();

			public void Initialize() => Reset( watcher.Restart );

			void Reset( Action action )
			{
				last.Assign( default(long) );
				action();
			}

			public long Time => watcher.ElapsedMilliseconds;

			public long Mark()
			{
				var result = watcher.ElapsedMilliseconds - last.Item;
				last.Assign( watcher.ElapsedMilliseconds );
				return result;
			}

			public void Dispose() => Reset( watcher.Stop );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose() => Mark( "Complete" );
	}
}