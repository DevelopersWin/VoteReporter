using JetBrains.dotMemoryUnit;
using PostSharp.Patterns.Model;
using System;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{
	

	/*public class ApplicationOutputCommand : OutputCommand
	{
		public ApplicationOutputCommand() : base( method => new InitializeMethodCommand( AssociatedContext.Default.Get( method ).Dispose ) ) {}
	}*/

	public static class MethodBaseExtensions
	{
		/*public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => AsCurrentContext( @this, new RecordingLoggerFactory( history, level ) );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this ) => AsCurrentContext( @this, new RecordingLoggerFactory() );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new InitializeMethodCommand().AsExecuted( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}*/

		// readonly static Func<object, ILogger> LoggerSource = DragonSpark.Diagnostics.Diagnostics.Logger.ToDelegate();

		/*public static IProfiler Profile( this MethodBase method, Action<string> output ) => 
			new ProfilerFactory( output, DragonSpark.Diagnostics.Diagnostics.History.Get( method ), LoggerSource ).Create( method );*/

		/*public static IProfiler Trace( this MethodBase method, Action<string> output )
		{
			var profiler = method.Profile( output );
			var result = profiler.AssociateForDispose( LoggerSource( method ).WithTracing() );
			return result;
		}*/
	}

	/*public class InitializeMethodCommand : AssignCommand<MethodBase>
	{
		readonly Action complete;
		readonly Action<Assembly> initialize;

		public InitializeMethodCommand() : this( Delegates.Empty ) {}

		public InitializeMethodCommand( Action complete ) : this( AssemblyInitializer.Instance.ToDelegate(), complete ) {}

		public InitializeMethodCommand( Action<Assembly> initialize, Action complete ) : this( ExecutionContext.Instance.Value, initialize, complete ) {}

		InitializeMethodCommand( IWritableStore<MethodBase> store, Action<Assembly> initialize, Action complete ) : base( store )
		{
			this.initialize = initialize;
			this.complete = complete;
		}

		public override void Execute( MethodBase parameter )
		{
			initialize( parameter.DeclaringType.Assembly );
			base.Execute( parameter );
		}

		protected override void OnDispose() => complete();
	}*/

	[Disposable]
	public abstract class TestCollectionBase : ITestOutputAware
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			Output = output;
			WriteLine = Output.WriteLine;
			DotMemoryUnitTestOutput.SetOutputMethod( WriteLine );
		}

		[Reference]
		public ITestOutputHelper Output { get; }

		protected Action<string> WriteLine { get; }

		protected virtual void Dispose( bool disposing ) {}
	}

	public interface ITestOutputAware
	{
		ITestOutputHelper Output { get; }
	}

	public static class Traits
	{
		public const string Category = nameof(Category);

		public static class Categories
		{
			public const string FileSystem = nameof(FileSystem), IoC = nameof(IoC), ServiceLocation = nameof(ServiceLocation), Performance = nameof(Performance), Memory = nameof(Memory);
		}
	}
}