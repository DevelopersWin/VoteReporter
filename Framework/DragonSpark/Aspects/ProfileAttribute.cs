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

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<MethodTimerController>( data => data.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<MethodTimerController>( data => data.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<MethodTimerController>( data => data.Dispose() );
	}

	public class TimerControllerFactory<T> : TimerControllerFactory where T : LogFactoryBase
	{
		public static TimerControllerFactory<T> Instance { get; } = new TimerControllerFactory<T>( Services.Get<ILogger>() );
		
		public TimerControllerFactory( ILogger logger ) : base( Services.Get<LogFactoryBase>( typeof(T) ).Create( logger ) ) {}
	}

	public class TimerControllerFactory : FactoryBase<MethodBase, ITimerController>
	{
		readonly Func<ITimer> timerSource;
		readonly Action<ProfilerEvent> handler;

		public TimerControllerFactory( Log log ) : this( () => new Timer(), new LoggerHandler( log ).Run ) {}

		public TimerControllerFactory( Func<ITimer> timerSource, Action<ProfilerEvent> handler )
		{
			this.timerSource = timerSource;
			this.handler = handler;
		}

		protected override ITimerController CreateItem( MethodBase parameter )
		{
			var controller = new MethodTimerController( parameter, timerSource() );
			var result = new TimerController( controller, new ProfileEventHandler( controller, handler ).Run );
			return result;
		}
	}

	public class LoggerHandler : DecoratedCommand<ProfilerEvent, MethodTimerTemplate>
	{
		public LoggerHandler( Log log ) : this( log, ConvertTemplate.Instance.Create ) {}

		public LoggerHandler( Log logger, Func<ProfilerEvent, MethodTimerTemplate> transform )
			: base( transform, new LoggerCommand( logger ) ) {}
	}

	public class ConvertTemplate : Converter<MethodProfilerEvent, MethodTimerTemplate>
	{
		public static ConvertTemplate Instance { get; } = new ConvertTemplate();

		public ConvertTemplate() : base( @event => new MethodTimerTemplate( @event ) ) {}
	}

	public class Converter<TFrom, TTo> : FactoryBase<ProfilerEvent, TTo> where TFrom : ProfilerEvent
	{
		readonly Func<TFrom, TTo> convert;

		public Converter( Func<TFrom, TTo> convert )
		{
			this.convert = convert;
		}

		protected override TTo CreateItem( ProfilerEvent parameter ) => parameter.AsTo<TFrom, TTo>( @from => convert( @from ) );
	}

	public class ProfilerEvent
	{
		public ProfilerEvent( string eventName, MethodBase method, ITimer timer )
		{
			EventName = eventName;
			Method = method;
			Timer = timer;
		}

		public string EventName { get; }

		public MethodBase Method { get; }

		public ITimer Timer { get; }
	}

	public class MethodTimerTemplate : LoggerTemplate
	{
		public MethodTimerTemplate( MethodProfilerEvent profilerEvent ) 
			: base(	"{Type}.{Method} [{Event}] - Wall time {WallTime} ms; Synchronous time {SynchronousTime} ms", 
					new object[] { profilerEvent.Method.DeclaringType.Name, profilerEvent.Method, profilerEvent.EventName, profilerEvent.Timer.Elapsed, profilerEvent.Timer.TrackingTimer.Elapsed } ) {}
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

	public class Formatter : Converter<MethodProfilerEvent, string>
	{
		public static Formatter Instance { get; } = new Formatter();

		Formatter() : base( @event => $"{@event.Method.DeclaringType.Name}.{@event.Method} [{@event.EventName}] - Wall time {@event.Timer.Elapsed.TotalMilliseconds} ms; Synchronous time {@event.Timer.TrackingTimer.Elapsed.TotalMilliseconds} ms" ) {}
	}

	public class MethodProfilerEvent : ProfilerEvent<MethodTimerController>
	{
		public MethodProfilerEvent( string eventName, MethodTimerController timer ) : base( eventName, timer.Method, timer ) {}
	}

	public abstract class ProfilerEvent<T> : ProfilerEvent where T : ITimer
	{
		protected ProfilerEvent( string eventName, MethodBase method, T timer ) : base( eventName, method, timer ) {}

		public new T Timer => (T)base.Timer;
	}

	/*public class MethodTimerControllerFormatter : FactoryBase<MethodTimerController, string>
	{
		public static MethodTimerControllerFormatter Instance { get; } = new MethodTimerControllerFormatter();

		protected override string CreateItem( MethodTimerController parameter ) => $"";
	}

	public class ObjectFormatter : FactoryBase<object, string>
	{
		public static ObjectFormatter Instance { get; } = new ObjectFormatter( MethodTimerControllerFormatter.Instance );

		readonly IEnumerable<IFactoryWithParameter> items;

		public ObjectFormatter( params IFactoryWithParameter[] items )
		{
			this.items = items;
		}

		protected override string CreateItem( object parameter )
		{
			var target = parameter.GetType();
			var result = items.WithFirst( item => item.CanCreate( parameter ) /*&& Factory.GetParameterType( item.GetType() ) == target#1#, factory => (string)factory.Create( parameter ) );
			return result;
		}
	}*/

	public class DebugOutputCommand : DelegatedCommand<string>
	{
		public static DebugOutputCommand Instance { get; } = new DebugOutputCommand();

		public DebugOutputCommand() : this( Specification<string>.Instance ) {}

		public DebugOutputCommand( ISpecification<string> specification ) : base( s => Debug.WriteLine( s ), specification ) {}
	}

	public class OutputMessageCommand : DecoratedCommand<ProfilerEvent, string>
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

	public interface ITimer
	{
		void Start();

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

		public void Update()
		{
			Total += current() - Item;
			Elapsed = time( Total );
		}

		ulong Total { get; set; }

		public virtual TimeSpan Elapsed { get; private set; }
	}

	public interface ITimerController : IDisposable
	{
		void Start();

		void Resume();

		void Pause();
	}

	public interface IMethodTimerController : ITimerController
	{
		MethodBase Method { get; }
	}

	public class ProfileEventHandler : ProfileEventHandler<MethodProfilerEvent>
	{
		public ProfileEventHandler( MethodTimerController controller, Action<ProfilerEvent> inner ) : base( s => new MethodProfilerEvent( s, controller ), new DelegatedCommand<ProfilerEvent>( inner ) ) {}
	}

	public abstract class ProfileEventHandler<T> : DecoratedCommand<string, ProfilerEvent> where T : ProfilerEvent
	{
		protected ProfileEventHandler( Func<string, T> transform, ICommand<ProfilerEvent> inner ) : base( transform, inner ) {}
	}

	public class TimerController : ITimerController
	{
		readonly ITimerController inner;
		readonly Action<string> handler;

		public TimerController( ITimerController inner, Action<string> handler )
		{
			this.inner = inner;
			this.handler = handler;
		}

		public void Start()
		{
			inner.Start();
			handler( nameof(Start) );
		}

		public void Resume()
		{
			inner.Resume();
			handler( nameof(Resume) );
		}

		public void Pause()
		{
			inner.Pause();
			handler( nameof(Pause) );
		}

		public void Dispose()
		{
			inner.Dispose();
			handler( "Completed" );
		}
	}

	public class MethodTimerController : Timer, IMethodTimerController
	{
		public MethodTimerController( MethodBase method ) : this( method, new Timer() ) {}

		public MethodTimerController( MethodBase method, ITimer trackingTimer )
		{
			Method = method;
			TrackingTimer = trackingTimer;
		}

		public MethodBase Method { get; }

		public ITimer TrackingTimer { get; }

		public override void Start()
		{
			base.Start();
			Resume();
		}

		public virtual void Resume() => TrackingTimer.Start();

		public virtual void Pause() => TrackingTimer.Update();

		protected override void OnDispose()
		{
			base.OnDispose();
			Pause();
		}
	}
}