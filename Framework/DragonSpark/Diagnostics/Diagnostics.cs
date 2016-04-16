using DragonSpark.Activation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Log = DragonSpark.Diagnostics.Logger.Log;

namespace DragonSpark.Diagnostics
{
	public static class ExceptionSupport
	{
		public static Exception Try( Action action ) => Try( Services.Get<TryContext>, action );

		public static Exception Try( this Func<TryContext> @this, Action action ) => @this().Try( action );

		public static void Process( this IExceptionHandler target, Exception exception ) => target.Handle( exception ).With( a => a.RethrowRecommended.IsTrue( () => { throw a.Exception; } ) );
	}

	public class ProfilerFactory<T> : ProfilerFactory<Timer, T> where T : Category.Factory {}

	public class ProfilerFactory<TTimer, TLog> : FactoryBase<MethodBase, IProfiler> where TLog : Category.Factory where TTimer : ITimer, new()
	{
		readonly ILogger logger;
		readonly Func<Log, Action<TimerEvent>> handlerSource;
		readonly ISessionTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> createSource;

		public ProfilerFactory() : this( TimerEventConverter.Instance.Create ) {}

		public ProfilerFactory( Func<TimerEvent, LoggerTemplate> templateSource ) : this( Services.Get<ILogger>(), new TTimer(), log => new Handler<TimerEvent>( log, templateSource ).Run ) {}

		public ProfilerFactory( ILogger logger, TTimer tracker, Func<Log, Action<TimerEvent>> handlerSource ) : this( logger, tracker, new SessionTimer( tracker ), handlerSource ) {}

		public ProfilerFactory( ILogger logger, TTimer tracker, ISessionTimer timer, Func<Log, Action<TimerEvent>> handlerSource ) : this( logger, timer, handlerSource, new HandlerFactory<TTimer>( timer, tracker ).Create ) {}

		public ProfilerFactory( ILogger logger, ISessionTimer timer, Func<Log, Action<TimerEvent>> handlerSource, Func<MethodBase, CreateProfilerEvent> createSource )
		{
			this.logger = logger;
			this.handlerSource = handlerSource;
			this.timer = timer;
			this.createSource = createSource;
		}

		protected override IProfiler CreateItem( MethodBase parameter )
		{
			var context = logger.ForContext( Constants.SourceContextPropertyName, $"{parameter.DeclaringType.Name}.{parameter}" );
			var log = Services.Get<IFactory<ILogger, Log>>( typeof(TLog) ).Create( context );
			var handler = handlerSource( log );

			var factory = new ProfilerFactory( timer, createSource, handler );
			var result = factory.Create( parameter );
			return result;
		}
	}

	public class ProfilerFactory : FactoryBase<MethodBase, IProfiler>
	{
		readonly ISessionTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> source;
		readonly Action<TimerEvent> handler;

		public ProfilerFactory( ISessionTimer timer, Func<MethodBase, CreateProfilerEvent> source, Action<TimerEvent> handler )
		{
			this.timer = timer;
			this.source = source;
			this.handler = handler;
		}

		protected override IProfiler CreateItem( MethodBase parameter )
		{
			ProfileEvent action = new TimerEventHandler( source( parameter ), handler ).Run;
			var command = new AmbientContextCommand<ProfileEvent>().ExecuteWith( action );
			var result = new Profiler( timer, action ).AssociateForDispose( command );
			return result;
		}
	}

	public delegate void ProfileEvent( string name );

	public static class Profile
	{
		public static void Event( string name ) => Ambient.GetCurrent<ProfileEvent>()( name );
	}

	public class HandlerFactory<T> : FactoryBase<MethodBase, CreateProfilerEvent> where T : ITimer
	{
		readonly ISessionTimer sessionTimer;
		readonly T timer;
		
		public HandlerFactory( ISessionTimer sessionTimer, T timer )
		{
			this.sessionTimer = sessionTimer;
			this.timer = timer;
		}

		protected override CreateProfilerEvent CreateItem( MethodBase parameter ) => s => new TimerEvent<T>( s, parameter, sessionTimer, timer );
	}

	public delegate TimerEvent CreateProfilerEvent( string eventName );

	public class TimerEvent
	{
		public TimerEvent( string eventName, MethodBase method, ITimer timer )
		{
			EventName = eventName;
			Method = method;
			Timer = timer;
		}

		public string EventName { get; }

		public MethodBase Method { get; }

		public ITimer Timer { get; }
	}

	public class TimerEventConverter : Converter<TimerEvent<Timer>, TimerEventTemplate>
	{
		public static TimerEventConverter Instance { get; } = new TimerEventConverter();

		public TimerEventConverter() : base( @event => new TimerEventTemplate( @event ) ) {}
	}

	public class TimerEventTemplate : LoggerTemplate
	{
		public TimerEventTemplate( TimerEvent<Timer> profilerEvent ) 
			: base(	"[{Event:l}] - Wall time {WallTime:ss':'fff} ms; Synchronous time {SynchronousTime:ss':'fff} ms", profilerEvent.EventName, profilerEvent.Timer.Elapsed, profilerEvent.Tracker.Elapsed ) {}
	}

	public class CompositeLoggerTemplate : LoggerTemplate
	{
		public CompositeLoggerTemplate( params LoggerTemplate[] templates ) : this( string.Empty, templates ) {}

		public CompositeLoggerTemplate( string separator, params LoggerTemplate[] templates ) : base( string.Join( separator, templates.Select( template => template.Template ) ), templates.SelectMany( template => template.Parameters ).ToArray() ) {}
	}

	public class LoggerTemplate
	{
		public LoggerTemplate( string template, params object[] parameters )
		{
			Template = template;
			Parameters = parameters;
		}

		public string Template { get; }

		public object[] Parameters { get; }
	}

	public class TimerEvent<T> : TimerEvent where T : ITimer
	{
		public TimerEvent( string eventName, MethodBase method, ITimer timer, T tracker ) : base( eventName, method, timer )
		{
			Tracker = tracker;
		}

		public T Tracker { get; }
	}

	/*public class OutputMessageCommand : DecoratedCommand<TimerEvent, string>
	{
		public static OutputMessageCommand Instance { get; } = new OutputMessageCommand( DebugOutputCommand.Instance );

		public OutputMessageCommand( ICommand<string> output ) : base( Formatter.Instance.Create, output ) {}
	}*/

	public class Timer : TimerBase
	{
		readonly static Stopwatch Stopwatch = Stopwatch.StartNew();

		public Timer() : base( () => (ulong)Stopwatch.ElapsedTicks, total => TimeSpan.FromSeconds( (double)total / Stopwatch.Frequency ) ) {}
	}

	public interface ITimer : IProcess
	{
		void Update();

		TimeSpan Elapsed { get; }
	}

	public abstract class TimerBase : FixedValue<ulong>, ITimer
	{
		readonly Func<ulong> current;
		readonly Func<ulong, TimeSpan> time;
		ulong total;

		protected TimerBase( Func<ulong> current, Func<ulong, TimeSpan> time )
		{
			this.current = current;
			this.time = time;
		}

		public virtual void Start()
		{
			Total = 0;
			Assign( current() );
		}

		public virtual void Update() => Total += current() - Item;

		ulong Total
		{
			get { return total; }
			set { Elapsed = time( total = value ); }
		}

		public virtual TimeSpan Elapsed { get; private set; }

		protected override void OnDispose() => Update();
	}

	public interface IProcess : IDisposable
	{
		void Start();
	}

	public interface IContinuation
	{
		void Resume();

		void Pause();
	}

	public class TimerEventHandler : DecoratedCommand<string, TimerEvent>
	{
		public TimerEventHandler( CreateProfilerEvent transform, Action<TimerEvent> inner ) : base( new Func<string, TimerEvent>( transform ), new DelegatedCommand<TimerEvent>( inner ) ) {}
	}

	public interface IProfiler : IProcess, IContinuation {}

	public class Profiler : IProfiler
	{
		readonly ISessionTimer inner;
		readonly ProfileEvent handler;
		readonly TimerEvents events;

		public Profiler( ISessionTimer inner, ProfileEvent handler ) : this( inner, handler, TimerEvents.Instance ) {}

		public Profiler( ISessionTimer inner, ProfileEvent handler, TimerEvents events )
		{
			this.inner = inner;
			this.handler = handler;
			this.events = events;
		}

		public void Start()
		{
			inner.Start();
			handler( events.Starting );
		}

		public void Resume()
		{
			inner.Resume();
			handler( events.Resuming );
		}

		public void Pause()
		{
			inner.Pause();
			handler( events.Paused );
		}

		public void Dispose()
		{
			inner.Dispose();
			handler( events.Completed );
		}
	}

	public interface ISessionTimer : ITimer, IContinuation {}

	public class SessionTimer : Timer, ISessionTimer
	{
		readonly ITimer timer;

		public SessionTimer( ITimer timer )
		{
			this.timer = timer;
		}

		public override void Start()
		{
			base.Start();
			Resume();
		}

		public void Resume() => timer.Start();

		public void Pause() => timer.Update();

		protected override void OnDispose()
		{
			Pause();
			base.OnDispose();
		}
	}

	/*public class ProfilerFactory : ConfiguringFactory<IProfileProcess>
	{
		public ProfilerFactory( ILogger logger, string context, PurgeLoggerHistoryFixedCommand purgeCommand ) 
			: base( () => new Profiler( logger, context ), new ConfigureProfilerCommand( purgeCommand ).Run ) {}
	}*/

	/*public class ConfigureProfilerCommand : CompositeCommand
	{
		readonly PurgeLoggerHistoryFixedCommand purge;

		public ConfigureProfilerCommand( [Required] PurgeLoggerHistoryFixedCommand purge ) : base( purge, StartProcessCommand.Instance )
		{
			this.purge = purge;
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			purge.ExecuteWith( this );
		}
	}*/

	public class StartProcessCommand : Command<IProcess>
	{
		public static StartProcessCommand Instance { get; } = new StartProcessCommand();

		protected override void OnExecute( IProcess parameter ) => parameter.Start();
	}

	/*
	public static class ProfilerExtensions
	{
		public static IProfileProcess Mark<T>( this IProfileProcess @this, string message )
		{
			@this.Event( $"[{typeof(T).Name}] {message}" );
			return @this;
		}
	}

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Profiler : IProfileProcess
	{
		readonly ILogger logger;

		readonly Tracker timer = new Tracker();

		public Profiler( [Required] ILogger logger, [NotEmpty] string context )
		{
			this.logger = logger.ForContext( Constants.SourceContextPropertyName, context );
		}

		public void Start()
		{
			Event( "Starting" );
			timer.Initialize();
		}

		/*public IProfileProcess New()
		{
			return new Profiler( logger );
		}#1#

		public void Event( string @event ) => logger.Information( "@ {Time:ss':'fff} ({Since:ss':'fff}): {Event:l}", timer.Time, timer.Mark(), @event );

		class Tracker : IDisposable
		{
			readonly Stopwatch watcher = Stopwatch.StartNew();

			readonly FixedValue<TimeSpan> last = new FixedValue<TimeSpan>();

			public void Initialize() => Reset( watcher.Restart );

			void Reset( Action action )
			{
				last.Assign( default(TimeSpan) );
				action();
			}

			public TimeSpan Time => watcher.Elapsed;

			public TimeSpan Mark()
			{
				var result = watcher.Elapsed - last.Item;
				last.Assign( watcher.Elapsed );
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
		protected virtual void OnDispose() => Event( "Complete" );
	}*/
}