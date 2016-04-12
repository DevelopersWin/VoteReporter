using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Diagnostics;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable]
	public class ProfileAttribute : OnMethodBoundaryAspect
	{
		public ProfileAttribute() : this( typeof(Factory) ) {}

		public ProfileAttribute( [OfFactoryType] Type factoryType )
		{
			FactoryType = factoryType;
		}

		Type FactoryType { get; set; }

		public override void OnEntry( MethodExecutionArgs args ) => args.MethodExecutionTag = Services.Get<IFactory<MethodBase, Performance.Data>>( FactoryType ).Create( args.Method ).With( data => data.Start() );

		public override void OnYield( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.Data>( data => data.Pause() );

		public override void OnResume( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.Data>( data => data.Resume() );

		public override void OnExit( MethodExecutionArgs args ) => args.MethodExecutionTag.As<Performance.Data>( data => data.Dispose() );

		class Factory : FactoryBase<MethodBase, Performance.Data>
		{
			readonly static Action<Performance.Data> Complete = new Performance.OutputMessageCommand( data => data.AsTo<Performance.Data, string>( Performance.Formatter.Instance.Create ), s => Debug.WriteLine( s ) ).Run;

			protected override Performance.Data CreateItem( MethodBase parameter ) => new Performance.Data( Complete, parameter );
		}
	}

	public static class Performance
	{
		public class Formatter : FactoryBase<Data, string>
		{
			public static Formatter Instance { get; } = new Formatter();

			protected override string CreateItem( Data parameter ) => 
				$"{parameter.Method.DeclaringType.Name}.{parameter.Method.Name} - Wall time {parameter.WallTime.Time.TotalMilliseconds} ms; Synchronous time {parameter.SynchronousTime.Time.TotalMilliseconds} ms";
		}

		public class OutputMessageCommand : Command<Data>
		{
			readonly Func<Data, string> formatter;
			readonly Action<string> output;

			public OutputMessageCommand( Action<string> output ) : this( Formatter.Instance.Create, output ) {}

			public OutputMessageCommand( Func<Data, string> formatter, Action<string> output )
			{
				this.formatter = formatter;
				this.output = output;
			}

			protected override void OnExecute( Data parameter )
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

			public void Initialize() => Assign( current() );

			public ulong Total { get; private set; }

			public virtual TimeSpan Time => TimeSpan.FromSeconds( (double)Total / Stopwatch.Frequency );

			public void Mark() => Total += current() - Item;
		}

		public class Data : IDisposable
		{
			readonly Action<Data> complete;

			public Data( Action<Data> complete, MethodBase method )
			{
				this.complete = complete;
				Method = method;
			}

			public MethodBase Method { get; }

			public Timer WallTime { get; } = new Timer();

			public Timer SynchronousTime { get; } = new Timer();

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