using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	// public delegate void EmitProfileEvent( string name );

	/*public static class Profile
	{
		public static void Event( string name ) => AmbientStack.GetCurrentItem<EmitProfileEvent>()( name );

		public static T Emit<T>( this T @this, string name )
		{
			Event( name );
			return @this;
		}
	}

	public class HandlerFactory<T> : Cache<MethodBase, CreateProfilerEvent> where T : ITimer
	{
		public HandlerFactory( ISessionTimer sessionTimer, T timer ) : base( new Owner( sessionTimer, timer ).Create ) {}

		class Owner
		{
			readonly ISessionTimer sessionTimer;
			readonly T timer;
		
			public Owner( ISessionTimer sessionTimer, T timer )
			{
				this.sessionTimer = sessionTimer;
				this.timer = timer;
			}

			public CreateProfilerEvent Create( MethodBase parameter ) => new Context( this, parameter ).Get;

			class Context
			{
				readonly Owner owner;
				readonly MethodBase parameter;
				
				public Context( Owner owner, MethodBase parameter )
				{
					this.owner = owner;
					this.parameter = parameter;
				}

				public TimerEvent Get( string name ) => new TimerEvent<T>( name, parameter, owner.sessionTimer, owner.timer );
			}
		}
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

	public class TimerEventConverter : ProjectedFactory<TimerEvent, TimerEvent<Timer>, TimerEventTemplate>
	{
		public static TimerEventConverter Instance { get; } = new TimerEventConverter();

		TimerEventConverter() : base( @event => new TimerEventTemplate( @event ) ) {}
	}*/

	/*public class TimerEventTemplate : LoggerTemplate
	{
		new const string Template = "[{Event:l}] - Wall time {WallTime:ss':'fff}; Synchronous time {SynchronousTime:ss':'fff}";

		public TimerEventTemplate( TimerEvent<Timer> profilerEvent ) 
			: base(	Template, profilerEvent.EventName, profilerEvent.Timer.Elapsed, profilerEvent.Tracker.Elapsed ) {}
	}*/

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

	/*public class TimerEvent<T> : TimerEvent where T : ITimer
	{
		public TimerEvent( string eventName, MethodBase method, ITimer timer, T tracker ) : base( eventName, method, timer )
		{
			Tracker = tracker;
		}

		public T Tracker { get; }
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

	public abstract class TimerBase : FixedSource<ulong>, ITimer
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

		public virtual void Update() => Total += current() - Get();

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

	/*public class TimerEventHandler : ProjectedCommand<string>
	{
		public TimerEventHandler( Action<TimerEvent> inner, CreateProfilerEvent projection ) : base( new DelegatedCommand<TimerEvent>( inner, new Projector<string, TimerEvent>( new Func<string, TimerEvent>( projection ) ).ToDelegate() ) ) {}
	}*/

	/*public interface ISessionTimer : ITimer, IContinuation {}

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

		public override TimeSpan Elapsed => timer.Elapsed;
	}
	
	public class StartProcessCommand : CommandBase<IProcess>
	{
		public static StartProcessCommand Instance { get; } = new StartProcessCommand();

		public override void Execute( IProcess parameter ) => parameter.Start();
	}*/
}