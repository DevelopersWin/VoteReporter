using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using Serilog;
using Serilog.Events;
using System;
using System.Reflection;

namespace DragonSpark.Diagnostics
{
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
}