using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Diagnostics;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.TypeSystem;
using JetBrains.dotMemoryUnit;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using Serilog;
using Serilog.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;
using ProfilerFactory = DragonSpark.Testing.Framework.Diagnostics.ProfilerFactory;

namespace DragonSpark.Testing.Framework
{
	public static class Defer
	{
		public static Task Run( Action action, object context )
		{
			var task = context as Task;
			if ( task != null )
			{
				var continueWith = task.ContinueWith( t => action() );
				// continueWith.ConfigureAwait( false );
				return continueWith;
				// task.ToObservable().Subscribe( unit => action() );
			}
			action();
			return null;
		}
	}

	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class ExecuteMethodAspect : MethodInterceptionAspect
	{
		public static ExecuteMethodAspect Instance { get; } = new ExecuteMethodAspect();

		ExecuteMethodAspect() {}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			new ApplicationOutputCommand().Run( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );

			args.ReturnValue = Defer.Run( ExecutionContextStore.Instance.Value.Dispose, args.ReturnValue );
		}
	}

	public class ApplicationOutputCommand : OutputCommand
	{
		public ApplicationOutputCommand() : base( method => new InitializeMethodCommand( method.Get( AssociatedContext.Property ).Dispose ) ) {}
	}

	public static class MethodBaseExtensions
	{
		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => AsCurrentContext( @this, new RecordingLoggerFactory( history, level ) );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this ) => AsCurrentContext( @this, new RecordingLoggerFactory() );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new InitializeMethodCommand().AsExecuted( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}

		public static IProfiler Profile( this MethodBase method, Action<string> output, ILoggerHistory history, ILogger logger ) => Profile( method, output, history, logger.Wrap() );

		public static IProfiler Profile( this MethodBase method, Action<string> output, ILoggerHistory history, Func<MethodBase, ILogger> loggerSource )
		{
			var profiler = new ProfilerFactory( output, history, loggerSource ).Create( method );
			var result = profiler.AssociateForDispose( DiagnosticProperties.Logger.Get( profiler ).WithTracing() );
			return result;
		}
	}

	public class InitializeMethodCommand : AssignValueCommand<MethodBase>
	{
		readonly Action complete;
		readonly Action<Assembly> initialize;

		public InitializeMethodCommand() : this( Delegates.Empty ) {}

		public InitializeMethodCommand( Action complete ) : this( AssemblyInitializer.Instance.Run, complete ) {}

		public InitializeMethodCommand( Action<Assembly> initialize, Action complete ) : this( ExecutionContextStore.Instance.Value, initialize, complete ) {}

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

		protected override void OnDispose()
		{
			// base.OnDispose();
			complete();
		}
	}

	[Disposable]
	public abstract class TestCollectionBase : ITestOutputAware
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			Output = output;
			DotMemoryUnitTestOutput.SetOutputMethod( Output.WriteLine );
		}

		[Reference]
		public ITestOutputHelper Output { get; }

		protected virtual void Dispose( bool disposing ) {}
	}

	public interface ITestOutputAware
	{
		ITestOutputHelper Output { get; }
	}

	public static class Traits
	{
		public const string Category = "Category";

		public static class Categories
		{
			public const string FileSystem = "FileSystem", IoC = "IoC", ServiceLocation = "ServiceLocation";
		}
	}
}