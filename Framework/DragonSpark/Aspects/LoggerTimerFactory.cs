using DragonSpark.Activation;
using DragonSpark.Extensions;
using Serilog;
using Serilog.Core;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public class LoggerTimerFactory<T> : LoggerTimerFactory<T, Timer> where T : LogFactoryBase {}

	public class LoggerTimerFactory<T, TTimer> : FactoryBase<MethodBase, ITimerController> where T : LogFactoryBase where TTimer : ITimer, new()
	{
		readonly ILogger logger;
		readonly Func<Log, Action<TimerEvent>> handlerSource;
		readonly IControlledTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> createSource;

		public LoggerTimerFactory() : this( ConvertTemplate.Instance.Create ) {}

		public LoggerTimerFactory( Func<TimerEvent, LoggerTemplate> templateSource ) : this( Services.Get<ILogger>(), new TTimer(), log => new LoggerHandler( log, templateSource ).Run ) {}

		public LoggerTimerFactory( ILogger logger, TTimer tracker, Func<Log, Action<TimerEvent>> handlerSource ) : this( logger, tracker, new ControlledTimer( tracker ), handlerSource ) {}

		public LoggerTimerFactory( ILogger logger, TTimer tracker, IControlledTimer timer, Func<Log, Action<TimerEvent>> handlerSource ) : this( logger, handlerSource, timer, new HandlerFactory<TTimer>( timer, tracker ).Create ) {}

		public LoggerTimerFactory( ILogger logger, Func<Log, Action<TimerEvent>> handlerSource, IControlledTimer timer, Func<MethodBase, CreateProfilerEvent> createSource )
		{
			this.logger = logger;
			this.handlerSource = handlerSource;
			this.timer = timer;
			this.createSource = createSource;
		}

		protected override ITimerController CreateItem( MethodBase parameter )
		{
			var context = logger.ForContext( Constants.SourceContextPropertyName, $"{parameter.DeclaringType.Name}.{parameter}" );
			var log = Services.Get<LogFactoryBase>( typeof(T) ).Create( context );
			var handler = handlerSource( log );
			var factory = new TimerControllerFactory( timer, createSource, handler );
			var result = factory.Create( parameter );
			return result;
		}
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

	public delegate TimerEvent CreateProfilerEvent( string eventName );
}