using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Diagnostics
{
	public static class ExceptionSupport
	{
		public static Exception Try( Action action ) => Try( Services.Get<TryContext>, action );

		public static Exception Try( this Func<TryContext> @this, Action action ) => @this().Try( action );

		public static void Process( this IExceptionHandler target, Exception exception ) => target.Handle( exception ).With( a => a.RethrowRecommended.IsTrue( () => { throw a.Exception; } ) );
	}

	public class ProfilerFactory : ProfilerFactoryBase<Timer>
	{
		public ProfilerFactory() : base( TimerEventConverter.Instance.Create ) {}
	}
	
	public abstract class ProfilerFactoryBase<TTimer> : FactoryBase<MethodBase, IProfiler> where TTimer : ITimer, new()
	{
		static LogEventLevel Level() => Configure.Get<Configuration>().Profiler.Level;

		readonly Func<MethodBase, ILogger> loggerSource;
		readonly Func<ILogger, Action<TimerEvent>> handlerSource;
		readonly ISessionTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> createSource;

		protected ProfilerFactoryBase( Func<TimerEvent, ILoggerTemplate> templateSource ) : this( Level(), templateSource ) {}

		protected ProfilerFactoryBase( LogEventLevel level, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( new MethodLoggerFactory( Services.Get<ILogger>() ).Create, level, templateSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( loggerSource, Level(), templateSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, LogEventLevel level, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( loggerSource, new TTimer(), log => new Handler<TimerEvent>( log, level, templateSource ).Run ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, TTimer tracker, Func<ILogger, Action<TimerEvent>> handlerSource ) : this( loggerSource, tracker, new SessionTimer( tracker ), handlerSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, TTimer tracker, ISessionTimer timer, Func<ILogger, Action<TimerEvent>> handlerSource ) : this( loggerSource, timer, handlerSource, new HandlerFactory<TTimer>( timer, tracker ).Create ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, ISessionTimer timer, Func<ILogger, Action<TimerEvent>> handlerSource, Func<MethodBase, CreateProfilerEvent> createSource )
		{
			this.loggerSource = loggerSource;
			this.handlerSource = handlerSource;
			this.timer = timer;
			this.createSource = createSource;
		}

		protected override IProfiler CreateItem( MethodBase parameter )
		{
			var logger = loggerSource( parameter );
			var handler = handlerSource( logger );

			var factory = new ProfilerSourceFactory( timer, createSource, handler );
			var result = factory.Create( parameter );
			return result;
		}
	}

	public class MethodLoggerFactory : FactoryBase<MethodBase, ILogger>
	{
		readonly ILogger logger;

		public MethodLoggerFactory( ILogger logger )
		{
			this.logger = logger;
		}

		[Freeze]
		protected override ILogger CreateItem( MethodBase parameter ) => logger.ForContext( Constants.SourceContextPropertyName, $"{parameter.DeclaringType.Name}.{parameter.Name}" );
	}

	public class ProfilerSourceFactory : FactoryBase<MethodBase, IProfiler>
	{
		readonly ISessionTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> source;
		readonly Action<TimerEvent> handler;

		public ProfilerSourceFactory( ISessionTimer timer, Func<MethodBase, CreateProfilerEvent> source, Action<TimerEvent> handler )
		{
			this.timer = timer;
			this.source = source;
			this.handler = handler;
		}

		protected override IProfiler CreateItem( MethodBase parameter )
		{
			EmitProfileEvent action = new TimerEventHandler( source( parameter ), handler ).Run;
			var command = new AmbientContextCommand<EmitProfileEvent>().ExecuteWith( new EmitProfileEvent( name =>
			{
				timer.Update();
				action( name );
			} ) );
			var result = new Profiler( timer, action ).AssociateForDispose( command );
			return result;
		}
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

	public class CompositeLoggerTemplate : LoggerTemplate
	{
		public CompositeLoggerTemplate( params ILoggerTemplate[] templates ) : this( string.Empty, templates ) {}

		public CompositeLoggerTemplate( string separator, params ILoggerTemplate[] templates ) : base( string.Join( separator, templates.Select( template => template.Template ) ), templates.SelectMany( template => template.Parameters ).ToArray() ) {}
	}

	/*public class LoggerTemplate : LoggerTemplate<Information>
	{
		public LoggerTemplate( string template, params object[] parameters ) : base( template, parameters ) {}
	}

	public class LoggerTemplate<T> : LoggerTemplateBase where T : CategoryFactory
	{
		public LoggerTemplate( string template, params object[] parameters ) : base( Activator.Instance.Activate<T>().Create, template, parameters ) {}
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

	/*public class LoggerTemplateWithLevel : ILoggerExceptionTemplate
	{
		public LoggerTemplateWithLevel( ) {}

		public string Template
		{
			get { return null; }
		}

		public object[] Parameters
		{
			get { return new object[] { }; }
		}

		public Exception Exception
		{
			get { return null; }
		}
	}*/

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

		// public Func<ILogger, Log> PreferredLog { get; }
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
		readonly EmitProfileEvent handler;
		readonly TimerEvents events;

		public Profiler( ISessionTimer inner, EmitProfileEvent handler ) : this( inner, handler, TimerEvents.Instance ) {}

		public Profiler( ISessionTimer inner, EmitProfileEvent handler, TimerEvents events )
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
	
	public class StartProcessCommand : Command<IProcess>
	{
		public static StartProcessCommand Instance { get; } = new StartProcessCommand();

		protected override void OnExecute( IProcess parameter ) => parameter.Start();
	}
}