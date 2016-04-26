using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using Serilog.Core;
using System;
using System.Reflection;
using PostSharp.Patterns.Threading;
using Xunit.Abstractions;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContext;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class ExecuteMethodAspect : MethodInterceptionAspect
	{
		public static ExecuteMethodAspect Instance { get; } = new ExecuteMethodAspect();

		ExecuteMethodAspect() {}

		public override void OnInvoke( MethodInterceptionArgs args ) => new ApplicationOutputCommand().Run( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );
	}

	public class ApplicationOutputCommand : OutputCommand
	{
		public ApplicationOutputCommand() : base( method => new AssignExecutionContextCommand( new AssociatedContext( method ).Value.Dispose ) ) {}
	}

	public static class MethodBaseExtensions
	{
		public static AssignExecutionContextCommand AsCurrentContext( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => AsCurrentContext( @this, new RecordingLoggerFactory( history, level ) );

		public static AssignExecutionContextCommand AsCurrentContext( this MethodBase @this ) => AsCurrentContext( @this, new RecordingLoggerFactory() );

		public static AssignExecutionContextCommand AsCurrentContext( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new AssignExecutionContextCommand().Executed( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<MethodBase>
	{
		readonly Action complete;
		readonly Action<Assembly> initialize;

		public AssignExecutionContextCommand() : this( () => {} ) {}

		public AssignExecutionContextCommand( Action complete ) : this( AssemblyInitializer.Instance.Run, ExecutionContext.Instance )
		{
			this.complete = complete;
		}

		public AssignExecutionContextCommand( Action<Assembly> initialize, IWritableStore<MethodBase> store ) : base( store )
		{
			this.initialize = initialize.Synchronized();
		}

		protected override void OnExecute( MethodBase parameter )
		{
			initialize( parameter.DeclaringType.Assembly );

			base.OnExecute( parameter );
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			complete();
		}
	}

	[Disposable]
	public abstract class TestCollectionBase : ITestOutputAware
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			Output = output;
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
			public const string Modularity = "Modularity", IoC = "IoC";
		}
	}
}