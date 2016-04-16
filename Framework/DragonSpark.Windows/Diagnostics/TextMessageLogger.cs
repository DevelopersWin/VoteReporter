using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using PostSharp.Serialization;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DragonSpark.Windows.Diagnostics
{
	public class EnrichFromLogContextCommand : EnrichCommandBase
	{
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.FromLogContext();
	}

	[PSerializable]
	public sealed class ProfileAttribute : Aspects.ProfileAttribute
	{
		public ProfileAttribute() : base( typeof(ProfilerFactory<Category.Debug>) ) {}
	}

	public class ProfilerFactory<T> : ProfilerFactory<Timer, T> where T : Category.Factory
	{
		public ProfilerFactory() : base( TimerEventConverter.Instance.Create ) {}
	}

	public class Timer : DragonSpark.Diagnostics.Timer
	{
		readonly FixedValue<CpuTime> threadTime = new FixedValue<CpuTime>();

		public Timer()
		{
			KernelTime = new ThreadTimer( () => threadTime.Item.Kernel );
			UserTime = new ThreadTimer( () => threadTime.Item.User );
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
			threadTime.Assign( ThreadTimeFactory.Instance.Create() );
			new[] { KernelTime, UserTime }.Each( action );
		}
	}

	public class ThreadTimer : TimerBase
	{
		public ThreadTimer( Func<ulong> current ) : base( current, total => TimeSpan.FromMilliseconds( total * 0.0001 ) ) {}
	}

	public class TimerEventConverter : Activation.Converter<TimerEvent<Timer>, LoggerTemplate>
	{
		public static TimerEventConverter Instance { get; } = new TimerEventConverter();

		public TimerEventConverter() : base( @event => new CompositeLoggerTemplate( 
			new DragonSpark.Diagnostics.TimerEventTemplate( new TimerEvent<DragonSpark.Diagnostics.Timer>( @event.EventName, @event.Method, @event.Timer, @event.Tracker ) ),
			new TimerEventTemplate( @event )
		) ) {}
	}

	public class TimerEventTemplate : LoggerTemplate
	{
		public TimerEventTemplate( TimerEvent<Timer> timerEvent ) : base( "CPU time: {CpuTime} ms", timerEvent.Tracker.CpuElapsed ) {}
	}

	public class CpuTime
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
		public static ThreadTimeFactory Instance { get; } = new ThreadTimeFactory();

		[DllImport( "kernel32.dll", SetLastError = true )]
		[SuppressUnmanagedCodeSecurity]
		static extern bool GetThreadTimes(IntPtr threadHandle, out ulong creationTime, out ulong exitTime, out ulong kernelTime, out ulong userTime);
 
		[DllImport( "kernel32.dll" )]
		[SuppressUnmanagedCodeSecurity]
		static extern IntPtr GetCurrentThread();

		protected override CpuTime CreateItem()
		{
			ulong creationTime, exitTime, kernel, user;
			GetThreadTimes( GetCurrentThread(), out creationTime, out exitTime, out kernel, out user );
			var result = new CpuTime( kernel, user );
			return result;
		}
	}

	public class AddSeqSinkCommand : AddSinkCommand
	{
		public AddSeqSinkCommand() : this( LogEventLevel.Verbose, 1000, null, null, null, null ) {}

		public AddSeqSinkCommand( LogEventLevel restrictedToMinimumLevel, int batchPostingLimit, TimeSpan? period, string apiKey, string bufferBaseFilename, long? bufferFileSizeLimitBytes ) : base( restrictedToMinimumLevel )
		{
			BatchPostingLimit = batchPostingLimit;
			Period = period;
			ApiKey = apiKey;
			BufferBaseFilename = bufferBaseFilename;
			BufferFileSizeLimitBytes = bufferFileSizeLimitBytes;
		}

		public int BatchPostingLimit { get; set; }
		public TimeSpan? Period { get; set; }
		public string ApiKey { get; set; }
		public string BufferBaseFilename { get; set; }
		public long? BufferFileSizeLimitBytes { get; set; }

		[Required]
		public Uri Endpoint { [return: Required]get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration )
			=> configuration.Seq( Endpoint.ToString(), RestrictedToMinimumLevel, BatchPostingLimit, Period, ApiKey, BufferBaseFilename, BufferFileSizeLimitBytes );
	}

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