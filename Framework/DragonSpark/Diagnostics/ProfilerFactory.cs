using DragonSpark.Activation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using Serilog;
using Serilog.Events;
using System;
using System.Reflection;
using Configure = DragonSpark.Configuration.Configure;

namespace DragonSpark.Diagnostics
{
	public class ProfilerFactory : ProfilerFactoryBase<Timer>
	{
		public ProfilerFactory() : base( TimerEventConverter.Instance.ToDelegate() ) {}
	}

	public abstract class ProfilerFactoryBase<TTimer> : FactoryWithSpecificationBase<MethodBase, IProfiler> where TTimer : ITimer, new()
	{
		static LogEventLevel Level() => Configure.Load<ProfilerLevelConfiguration>().Value;

		readonly Func<MethodBase, ILogger> loggerSource;
		readonly Func<ILogger, Action<TimerEvent>> handlerSource;
		readonly ISessionTimer timer;
		readonly Func<MethodBase, CreateProfilerEvent> createSource;

		protected ProfilerFactoryBase( Func<TimerEvent, ILoggerTemplate> templateSource ) : this( Level(), templateSource ) {}

		protected ProfilerFactoryBase( LogEventLevel level, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( DiagnosticProperties.Logger.Get, level, templateSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( loggerSource, Level(), templateSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, LogEventLevel level, Func<TimerEvent, ILoggerTemplate> templateSource ) : this( loggerSource, new TTimer(), log => new Handler<TimerEvent>( log, level, templateSource ).Execute ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, TTimer tracker, Func<ILogger, Action<TimerEvent>> handlerSource ) : this( loggerSource, tracker, new SessionTimer( tracker ), handlerSource ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, TTimer tracker, ISessionTimer timer, Func<ILogger, Action<TimerEvent>> handlerSource ) : this( loggerSource, timer, handlerSource, new HandlerFactory<TTimer>( timer, tracker ).Get ) {}

		protected ProfilerFactoryBase( Func<MethodBase, ILogger> loggerSource, ISessionTimer timer, Func<ILogger, Action<TimerEvent>> handlerSource, Func<MethodBase, CreateProfilerEvent> createSource )
		{
			this.loggerSource = loggerSource;
			this.handlerSource = handlerSource;
			this.timer = timer;
			this.createSource = createSource;
		}

		public override IProfiler Create( MethodBase parameter )
		{
			var logger = loggerSource( parameter );
			var handler = handlerSource( logger );

			// TODO: YUCK!
			EmitProfileEvent action = new TimerEventHandler( handler, createSource( parameter ) ).Execute;
			var command = new AmbientStackCommand<EmitProfileEvent>().AsExecuted( new EmitProfileEvent( name =>
																										   {
																											   timer.Update();
																											   action( name );
																										   } ) );
			var result = new Profiler( timer, action ).AssociateForDispose( command );
			DiagnosticProperties.Logger.Set( result, logger );
			return result;
		}
	}
}