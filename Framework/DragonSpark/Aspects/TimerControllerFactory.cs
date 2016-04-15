using DragonSpark.Activation;
using DragonSpark.Extensions;
using Serilog;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public class TimerControllerFactory<T> : TimerControllerFactory<T, Timer> where T : LogFactoryBase
	{}

	public class TimerControllerFactory<T, TTimer> : TimerControllerFactory where T : LogFactoryBase where TTimer : ITimer, new()
	{
		public TimerControllerFactory() : this( Services.Get<ILogger>() ) {}

		public TimerControllerFactory( ILogger logger ) : this( Services.Get<LogFactoryBase>( typeof(T) ).Create( logger ) ) {}

		public TimerControllerFactory( Log log ) : this( new LoggerHandler( log ).Run ) {}

		public TimerControllerFactory( Action<TimerEvent> handler ) : this( new TTimer(), handler ) {}

		public TimerControllerFactory( TTimer tracker, Action<TimerEvent> handler ) : this( tracker, new ControlledTimer( tracker ), handler ) {}

		public TimerControllerFactory( TTimer tracker, IControlledTimer timer, Action<TimerEvent> handler ) : base( timer, new HandlerFactory<TTimer>( timer, tracker ).Create, handler ) {}
	}

	public class HandlerFactory<T> : FactoryBase<MethodBase, CreateProfilerEvent> where T : ITimer
	{
		readonly IControlledTimer timer;
		readonly T tracker;
		
		public HandlerFactory( IControlledTimer timer, T tracker )
		{
			this.timer = timer;
			this.tracker = tracker;
		}

		protected override CreateProfilerEvent CreateItem( MethodBase parameter ) => s => new TimerEvent<T>( s, parameter, timer, tracker );
	}

	public class TimerControllerFactory : FactoryBase<MethodBase, ITimerController>
	{
		readonly IControlledTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> source;
		readonly Action<TimerEvent> handler;

		public TimerControllerFactory( IControlledTimer timer, Func<MethodBase, CreateProfilerEvent> source, Action<TimerEvent> handler )
		{
			this.timer = timer;
			this.source = source;
			this.handler = handler;
		}

		protected override ITimerController CreateItem( MethodBase parameter ) => new TimerController( timer, new ProfileEventHandler( source( parameter ), handler ).Run );
	}

	public delegate TimerEvent CreateProfilerEvent( string eventName );
}