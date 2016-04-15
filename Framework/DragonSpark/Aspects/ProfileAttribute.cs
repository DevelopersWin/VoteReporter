using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Serialization;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable]
	public class ProfileAttribute : OnMethodBoundaryAspect
	{
		public ProfileAttribute() : this( typeof(TimerControllerFactory<LoggerDebugFactory>) ) {}

		public ProfileAttribute( [OfFactoryType] Type factoryType )
		{
			FactoryType = factoryType;
		}

		Type FactoryType { get; set; }

		IFactory<MethodBase, ITimerController> Factory { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Factory = Services.Get<IFactory<MethodBase, ITimerController>>( FactoryType );

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = Factory.Create( args.Method ).With( data => data.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IControlledTimer>( data => data.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IControlledTimer>( data => data.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<IControlledTimer>( data => data.Dispose() );
	}

	public class LoggerHandler : DecoratedCommand<TimerEvent, LoggerTemplate>
	{
		public LoggerHandler( Log log ) : this( log, ConvertTemplate.Instance.Create ) {}

		public LoggerHandler( Log logger, Func<TimerEvent, LoggerTemplate> transform )
			: base( transform, new LoggerCommand( logger ) ) {}
	}

	public class ConvertTemplate : Converter<TimerEvent<Timer>, TimerEventTemplate>
	{
		public static ConvertTemplate Instance { get; } = new ConvertTemplate();

		public ConvertTemplate() : base( @event => new TimerEventTemplate( @event ) ) {}
	}

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

	public class TimerEventTemplate : LoggerTemplate
	{
		public TimerEventTemplate( TimerEvent<Timer> profilerEvent ) 
			: base(	"{Type}.{Method} [{Event}] - Wall time {WallTime} ms; Synchronous time {SynchronousTime} ms", 
					new object[] { profilerEvent.Method.DeclaringType.Name, profilerEvent.Method, profilerEvent.EventName, profilerEvent.Timer.Elapsed, profilerEvent.Tracker.Elapsed } ) {}
	}

	public class LoggerTemplate
	{
		public LoggerTemplate( string template, object[] parameters )
		{
			Template = template;
			Parameters = parameters;
		}

		public string Template { get; }

		public object[] Parameters { get; }
	}

	public class LoggerCommand : Command<LoggerTemplate>
	{
		readonly Log log;

		public LoggerCommand( Log log ) 
		{
			this.log = log;
		}

		protected override void OnExecute( LoggerTemplate parameter ) => log( parameter.Template, parameter.Parameters );
	}

	public delegate void Log( string template, object[] parameters );

	public abstract class LogFactoryBase : FactoryBase<ILogger, Log> {}

	public class LoggerDebugFactory : LogFactoryBase
	{
		public static LoggerDebugFactory Instance { get; } = new LoggerDebugFactory();

		protected override Log CreateItem( ILogger parameter ) => parameter.Debug;
	}

	public class LoggerInformationFactory : LogFactoryBase
	{
		public static LoggerInformationFactory Instance { get; } = new LoggerInformationFactory();

		protected override Log CreateItem( ILogger parameter ) => parameter.Information;
	}

	public class LoggerWarningFactory : LogFactoryBase
	{
		public static LoggerWarningFactory Instance { get; } = new LoggerWarningFactory();

		protected override Log CreateItem( ILogger parameter ) => parameter.Warning;
	}

	public class LoggerErrorFactory : LogFactoryBase
	{
		public static LoggerErrorFactory Instance { get; } = new LoggerErrorFactory();

		protected override Log CreateItem( ILogger parameter ) => parameter.Error;
	}

	public class LoggerFatalFactory : LogFactoryBase
	{
		public static LoggerFatalFactory Instance { get; } = new LoggerFatalFactory();

		protected override Log CreateItem( ILogger parameter ) => parameter.Fatal;
	}

	public class Formatter : Converter<TimerEvent<Timer>, string>
	{
		public static Formatter Instance { get; } = new Formatter();

		Formatter() : base( @event => $"{@event.Method.DeclaringType.Name}.{@event.Method} [{@event.EventName}] - Wall time {@event.Timer.Elapsed.TotalMilliseconds} ms; Synchronous time {@event.Tracker.Elapsed.TotalMilliseconds} ms" ) {}
	}

	/*public class ProfilerEvent : TimerEvent<Timer>
	{
		public ProfilerEvent( string eventName, MethodBase method, ITimer timer, Timer tracker ) : base( eventName, method, timer, tracker ) {}
	}*/

	public class TimerEvent<T> : TimerEvent where T : ITimer
	{
		public TimerEvent( string eventName, MethodBase method, ITimer timer, T tracker ) : base( eventName, method, timer )
		{
			Tracker = tracker;
		}

		public T Tracker { get; }
	}

	public class DebugOutputCommand : DelegatedCommand<string>
	{
		public static DebugOutputCommand Instance { get; } = new DebugOutputCommand();

		public DebugOutputCommand() : this( Specification<string>.Instance ) {}

		public DebugOutputCommand( ISpecification<string> specification ) : base( s => Debug.WriteLine( s ), specification ) {}
	}

	public class OutputMessageCommand : DecoratedCommand<TimerEvent, string>
	{
		public static OutputMessageCommand Instance { get; } = new OutputMessageCommand();

		public OutputMessageCommand() : this( DebugOutputCommand.Instance ) {}

		public OutputMessageCommand( ICommand<string> output ) : base( Formatter.Instance.Create, output ) {}
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

	public abstract class TimerBase : FixedValue<ulong>, ITimer
	{
		readonly Func<ulong> current;
		readonly Func<ulong, TimeSpan> time;

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

		public virtual void Update()
		{
			Total += current() - Item;
			Elapsed = time( Total );
		}

		ulong Total { get; set; }

		public virtual TimeSpan Elapsed { get; private set; }
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

	/*public interface IMethodTimer : IControlledTimer
	{
		MethodBase Method { get; }
	}*/

	/*public class ProfileEventHandler : ProfileEventHandler<MethodProfilerEvent>
	{
		public ProfileEventHandler( IMethodTimer timer, Action<TimerEvent> inner ) : base( s => new MethodProfilerEvent( s, timer ), new DelegatedCommand<TimerEvent>( inner ) ) {}
	}*/

	public class ProfileEventHandler : DecoratedCommand<string, TimerEvent>
	{
		public ProfileEventHandler( CreateProfilerEvent transform, Action<TimerEvent> inner ) : base( new Func<string, TimerEvent>( transform ), new DelegatedCommand<TimerEvent>( inner ) ) {}
	}

	public class TimerEvents
	{
		public TimerEvents() : this ( nameof(Starting), nameof(Paused), nameof(Resuming), nameof(Completed) ) {}

		public TimerEvents( string starting, string paused, string resuming, string completed )
		{
			Starting = starting;
			Paused = paused;
			Resuming = resuming;
			Completed = completed;
		}

		public string Starting { get; }
		public string Paused { get; }
		public string Resuming { get; }
		public string Completed { get; }
	}

	public interface ITimerController : IProcess, IContinuation {}

	public class TimerController : ITimerController
	{
		readonly IControlledTimer inner;
		readonly Action<string> handler;
		readonly TimerEvents events;

		public TimerController( IControlledTimer inner, Action<string> handler ) : this( inner, handler, new TimerEvents() ) {}

		public TimerController( IControlledTimer inner, Action<string> handler, TimerEvents events )
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

	public interface IControlledTimer : ITimer, IContinuation
	{
	}

	public class ControlledTimer : Timer, IControlledTimer
	{
		readonly ITimer tracker;

		public ControlledTimer( ITimer tracker )
		{
			this.tracker = tracker;
		}

		public override void Start()
		{
			base.Start();
			Resume();
		}

		public void Resume() => tracker.Start();

		public void Pause() => tracker.Update();

		protected override void OnDispose()
		{
			Pause();
			base.OnDispose();
		}
	}

	/*public class MethodTimer : MethodTimer<Timer>
	{
		public MethodTimer( MethodBase method ) : this( method, new Timer() ) {}

		public MethodTimer( MethodBase method, Timer timer ) : base( method, timer ) {}
	}

	public abstract class MethodTimer<T> : MethodAwareControlledTimmer where T : ITimer
	{
		protected MethodTimer( MethodBase method, T timer ) : base( method, timer ) {}

		public new T Tracker => (T)base.Tracker;
	}*/

	/*public class MethodAwareControlledTimer : ControlledTimer, IMethodTimer
	{
		public MethodAwareControlledTimer( MethodBase method, ITimer timer ) : base( timer )
		{
			Method = method;
		}

		public MethodBase Method { get; }
	}*/
}