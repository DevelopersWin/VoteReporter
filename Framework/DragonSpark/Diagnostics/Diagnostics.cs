using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics
{
	public class MethodLoggerFactory : FactoryBase<MethodBase, ILogger>
	{
		public static MethodLoggerFactory Instance { get; } = new MethodLoggerFactory();

		readonly Func<ILogger> source;

		public MethodLoggerFactory() : this( Services.Get<ILogger> ) {}

		public MethodLoggerFactory( Func<ILogger> source )
		{
			this.source = source;
		}

		[Freeze]
		protected override ILogger CreateItem( MethodBase parameter ) => source().ForSource( parameter );
	}

	public delegate void EmitProfileEvent( string name );

	public static class Profile
	{
		public static void Event( string name ) => Ambient.GetCurrent<EmitProfileEvent>()( name );

		public static T Emit<T>( this T @this, string name )
		{
			Event( name );
			return @this;
		}
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
			: base(	"[{Event:l}] - Wall time {WallTime:ss':'fff}; Synchronous time {SynchronousTime:ss':'fff}", profilerEvent.EventName, profilerEvent.Timer.Elapsed, profilerEvent.Tracker.Elapsed ) {}
	}

	public interface ILoggerExceptionTemplate : ILoggerTemplate
	{
		Exception Exception { get; }
	}

	public interface ILoggerTemplate
	{
		string Template { get; }

		object[] Parameters { get; }

		LogEventLevel IntendedLevel { get; }
	}

	public class ExceptionLoggerTemplate : LoggerTemplate, ILoggerExceptionTemplate
	{
		public ExceptionLoggerTemplate( Exception exception, string template, params object[] parameters ) : this( exception, LogEventLevel.Information, template, parameters ) {}

		public ExceptionLoggerTemplate( Exception exception, LogEventLevel intendedLevel, string template, params object[] parameters ) : base( intendedLevel, template, parameters )
		{
			Exception = exception;
		}

		public Exception Exception { get; }
	}

	public class CompositeLoggerTemplate : LoggerTemplate
	{
		public CompositeLoggerTemplate( params ILoggerTemplate[] templates ) : this( string.Empty, templates ) {}

		public CompositeLoggerTemplate( string separator, params ILoggerTemplate[] templates ) : base( string.Join( separator, templates.Select( template => template.Template ) ), templates.SelectMany( template => template.Parameters ).ToArray() ) {}
	}

	public class LoggerTemplate : ILoggerTemplate
	{
		protected LoggerTemplate( string template, params object[] parameters ) : this( LogEventLevel.Information, template, parameters ) {}

		protected LoggerTemplate( LogEventLevel intendedLevel, string template, params object[] parameters )
		{
			IntendedLevel = intendedLevel;
			Template = template;
			Parameters = parameters;
		}

		public LogEventLevel IntendedLevel { get; }
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

	public abstract class TimerBase : FixedStore<ulong>, ITimer
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

		public virtual void Update() => Total += current() - Value;

		ulong Total
		{
			get { return total; }
			set { Elapsed = time( total = value ); }
		}

		public virtual TimeSpan Elapsed { get; private set; }

		protected override void OnDispose()
		{
			Update();
			base.OnDispose();
		}
	}

	public class TimerEventHandler : BoxedCommand<string>
	{
		public TimerEventHandler( CreateProfilerEvent projection, Action<TimerEvent> inner ) : base( new DelegatedCommand<TimerEvent>( inner, new Projector<string,TimerEvent>( new Func<string, TimerEvent>( projection ) ) )  ) {}
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
	
	public class StartProcessCommand : CommandBase<IProcess>
	{
		public static StartProcessCommand Instance { get; } = new StartProcessCommand();

		public override void Execute( IProcess parameter ) => parameter.Start();
	}
}