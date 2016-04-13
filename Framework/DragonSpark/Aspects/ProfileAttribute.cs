using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
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
		public ProfileAttribute() : this( typeof(DataLoggerCommand<LoggerDebugFactory>) ) {}

		public ProfileAttribute( [OfFactoryType] Type commandType )
		{
			CommandType = commandType;
		}

		Type CommandType { get; set; }

		ICommand<Performance.MethodExecution> Command { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Command = Services.Get<ICommand<Performance.MethodExecution>>( CommandType );

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = new Performance.MethodExecution( args.Method, Command.Run ).With( data => data.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.MethodExecution>( data => data.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.MethodExecution>( data => data.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.MethodExecution>( data => data.Dispose() );
	}

	/*class DebugDataFactory : FactoryBase<MethodBase, Performance.Data>
	{
		protected override Performance.Data CreateItem( MethodBase parameter ) => new Performance.Data( Performance.DebugMessageCommand.Instance.Execute, parameter );
	}*/

	/*public class LoggerDataFactory<T> : FactoryBase<MethodBase, Performance.Data> where T : LogFactoryBase
	{
		public static LoggerDataFactory<T> Instance { get; } = new LoggerDataFactory<T>();

		readonly ILogger logger;

		public LoggerDataFactory() : this( Services.Get<ILogger>() ) {}

		public LoggerDataFactory( ILogger logger )
		{
			this.logger = logger;
		}

		protected override Performance.Data CreateItem( MethodBase parameter ) => new Performance.Data( new DataLoggerCommand<T>( logger ).Run, parameter );
	}*/

	public class DataLoggerCommand<T> : DataLoggerCommand where T : LogFactoryBase
	{
		public static DataLoggerCommand<T> Instance { get; } = new DataLoggerCommand<T>( Services.Get<ILogger>() );

		public DataLoggerCommand( ILogger logger ) : base( Services.Get<LogFactoryBase>( typeof(T) ).Create( logger ) ) {}
	}

	public class DataLoggerCommand : Command<Performance.MethodExecution>
	{
		readonly Log log;

		public DataLoggerCommand( Log log )
		{
			this.log = log;
		}

		protected override void OnExecute( Performance.MethodExecution parameter ) => new LogDataCommand( parameter ).Run( log );
	}

	public class LogDataCommand : TemplatedLoggerActionCommand
	{
		public LogDataCommand( Performance.MethodExecution execution ) : base( "{Type}.{Method} - Wall time {WallTime} ms; Synchronous time {SynchronousTime} ms", new object[] { execution.Method.DeclaringType.Name, execution.Method, execution.WallTime.Time, execution.SynchronousTime.Time } ) {}
	}

	public class TemplatedLoggerActionCommand : Command<Log>
	{
		readonly string template;
		readonly object[] parameters;

		public TemplatedLoggerActionCommand( string template, object[] parameters )
		{
			this.template = template;
			this.parameters = parameters;
		}

		protected override void OnExecute( Log parameter ) => parameter( template, parameters );
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

	public class DebugMessageCommand : Performance.OutputMessageCommand
	{
		public static DebugMessageCommand Instance { get; } = new DebugMessageCommand();

		public DebugMessageCommand() : this( Performance.Formatter.Instance.Create ) {}

		public DebugMessageCommand( Func<Performance.MethodExecution, string> formatter ) : base( formatter, s => Debug.WriteLine( s ) ) {}
	}

	public static class Performance
	{
		public class Formatter : FactoryBase<MethodExecution, string>
		{
			public static Formatter Instance { get; } = new Formatter();

			protected override string CreateItem( MethodExecution parameter ) => 
				$"{parameter.Method.DeclaringType.Name}.{parameter.Method} - Wall time {parameter.WallTime.Time.TotalMilliseconds} ms; Synchronous time {parameter.SynchronousTime.Time.TotalMilliseconds} ms";
		}

		public class OutputMessageCommand : Command<MethodExecution>
		{
			readonly Func<MethodExecution, string> formatter;
			readonly Action<string> output;

			public OutputMessageCommand( Action<string> output ) : this( Formatter.Instance.Create, output ) {}

			public OutputMessageCommand( Func<MethodExecution, string> formatter, Action<string> output )
			{
				this.formatter = formatter;
				this.output = output;
			}

			protected override void OnExecute( MethodExecution parameter )
			{
				var message = formatter( parameter );
				output( message );
			}
		}

		public class Timer : TimerBase
		{
			readonly static Stopwatch Stopwatch = Stopwatch.StartNew();

			public Timer() : base( () => (ulong)Stopwatch.ElapsedTicks ) {}
		}

		public abstract class TimerBase : FixedValue<ulong>
		{
			readonly Func<ulong> current;

			protected TimerBase( Func<ulong> current )
			{
				this.current = current;
			}

			public virtual void Initialize() => Assign( current() );

			public ulong Total { get; private set; }

			public virtual TimeSpan Time => TimeSpan.FromSeconds( (double)Total / Stopwatch.Frequency );

			public virtual void Mark() => Total += current() - Item;
		}

		public class MethodExecution : MethodExecution<Timer>
		{
			public MethodExecution( MethodBase method, Action<MethodExecution<Timer>> complete ) : base( method, complete ) {}
		}

		public class MethodExecution<T> : IDisposable where T : TimerBase, new()
		{
			readonly Action<MethodExecution<T>> complete;

			public MethodExecution( MethodBase method, Action<MethodExecution<T>> complete )
			{
				Method = method;
				this.complete = complete;
			}

			public MethodBase Method { get; }

			public Timer WallTime { get; } = new Timer();

			public T SynchronousTime { get; } = new T();

			public virtual void Start()
			{
				WallTime.Initialize();
 
				Resume();
			}
 
			public virtual void Resume() => SynchronousTime.Initialize();

			public virtual void Pause() => SynchronousTime.Mark();

			public virtual void Dispose()
			{
				Pause();
 
				WallTime.Mark();

				complete( this );
			}
		}
	}
}