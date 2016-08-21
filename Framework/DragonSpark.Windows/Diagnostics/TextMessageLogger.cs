using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Destructurers;
using System.Composition;

namespace DragonSpark.Windows.Diagnostics
{
	public sealed class ApplyExceptionDetails : TransformerBase<LoggerConfiguration>
	{
		[Export( typeof(ITransformer<LoggerConfiguration>) )]
		public static ApplyExceptionDetails Default { get; } = new ApplyExceptionDetails();
		ApplyExceptionDetails() {}

		public override LoggerConfiguration Get( LoggerConfiguration parameter ) => parameter.Enrich.WithExceptionDetails( new SuppliedAndExportedItems<IExceptionDestructurer>( ExceptionEnricher.DefaultDestructurers ) );
	}

	/*public class ProfilerFactory : ProfilerFactoryBase<Timer>
	{
		readonly static Func<TimerEvent, ILoggerTemplate> TemplateSource = TimerEventConverter.Default.Create;

		public ProfilerFactory() : base( TemplateSource ) {}

		public ProfilerFactory( Func<MethodBase, ILogger> loggerSource ) : base( loggerSource, TemplateSource ) {}
	}*/

	/*public class Timer : DragonSpark.Diagnostics.Timer
	{
		readonly FixedStore<CpuTime> threadTime = new FixedStore<CpuTime>();
		readonly ThreadTimer[] all;

		public Timer()
		{
			KernelTime = new ThreadTimer( () => threadTime.Value.Kernel );
			UserTime = new ThreadTimer( () => threadTime.Value.User );
			all = new[] { KernelTime, UserTime };
		}

		ThreadTimer KernelTime { get; }

		ThreadTimer UserTime { get; }

		public TimeSpan CpuElapsed => KernelTime.Elapsed + UserTime.Elapsed;

		public override void Start()
		{
			base.Start();
			Update( time => time.Start() );
		}

		public override void Update()
		{
			base.Update();
			Update( time => time.Update() );
		}

		void Update( Action<ThreadTimer> action )
		{
			threadTime.Assign( ThreadTimeFactory.Default.Create() );
			all.Each( action );
		}
	}

	public class ThreadTimer : TimerBase
	{
		public ThreadTimer( Func<ulong> current ) : base( current, total => TimeSpan.FromMilliseconds( total * 0.0001 ) ) {}
	}*/

	/*public class TimerEventConverter : ProjectedSource<TimerEvent, TimerEvent<Timer>, LoggerTemplate>
	{
		public static TimerEventConverter Default { get; } = new TimerEventConverter();

		public TimerEventConverter() : base( @event => new CompositeLoggerTemplate( "; ",
			new DragonSpark.Diagnostics.TimerEventTemplate( new TimerEvent<DragonSpark.Diagnostics.Timer>( @event.EventName, @event.Method, @event.Timer, @event.Tracker ) ),
			new TimerEventTemplate( @event )
		) ) {}
	}

	public class TimerEventTemplate : LoggerTemplate
	{
		public TimerEventTemplate( TimerEvent<Timer> timerEvent ) : base( "CPU time: {CpuTime:ss':'fff}", timerEvent.Tracker.CpuElapsed ) {}
	}
*/
	/*public class CpuTime
	{
		public CpuTime( ulong kernel, ulong user )
		{
			Kernel = kernel;
			User = user;
		}

		public ulong Kernel { get; }
		public ulong User { get; }
	}

	public class ThreadTimeFactory : FactoryBase<CpuTime>
	{
		public static ThreadTimeFactory Default { get; } = new ThreadTimeFactory();

		[DllImport( "kernel32.dll", SetLastError = true )]
		[SuppressUnmanagedCodeSecurity]
		static extern bool GetThreadTimes(IntPtr threadHandle, out ulong creationTime, out ulong exitTime, out ulong kernelTime, out ulong userTime);
 
		[DllImport( "kernel32.dll" )]
		[SuppressUnmanagedCodeSecurity]
		static extern IntPtr GetCurrentThread();

		public override CpuTime Create()
		{
			ulong creationTime, exitTime, kernel, user;
			GetThreadTimes( GetCurrentThread(), out creationTime, out exitTime, out kernel, out user );
			var result = new CpuTime( kernel, user );
			return result;
		}
	}*/

	public class AddConsoleSinkCommand : AddSinkCommand
	{
		public AddConsoleSinkCommand() : this( "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", LogEventLevel.Verbose ) {}

		public AddConsoleSinkCommand( [NotEmpty]string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration )
			=> configuration.ColoredConsole( RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}

	public class AddTraceSinkCommand : AddSinkCommand
	{
		public AddTraceSinkCommand() : this( "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", LogEventLevel.Verbose ) {}

		public AddTraceSinkCommand( [NotEmpty]string outputTemplate, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
		}

		[NotEmpty]
		public string OutputTemplate { [return: NotEmpty]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.Trace( RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}
	
	public class AddRollingFileSinkCommand : AddSinkCommand
	{
		public AddRollingFileSinkCommand() : this( "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", 1073741824, 31, LogEventLevel.Verbose ) {}

		public AddRollingFileSinkCommand( [NotEmpty]string outputTemplate, long fileSizeLimitBytes, int retainedFileCountLimit, LogEventLevel restrictedToMinimumLevel ) : base( restrictedToMinimumLevel )
		{
			OutputTemplate = outputTemplate;
			FileSizeLimitBytes = fileSizeLimitBytes;
			RetainedFileCountLimit = retainedFileCountLimit;
		}

		[NotEmpty]
		public string PathFormat { [return: NotEmpty]get; set; }

		public string OutputTemplate { get; set; }

		public long FileSizeLimitBytes { get; set; }

		public int RetainedFileCountLimit { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) 
			=> configuration.RollingFile( PathFormat, RestrictedToMinimumLevel, OutputTemplate, FormatProvider, FileSizeLimitBytes, RetainedFileCountLimit );
	}
}