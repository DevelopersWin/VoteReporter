using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using Serilog;
using Serilog.Core;
using System;
using System.Reflection;
using Xunit.Abstractions;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContext;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public class AssignExecutionContextAspect : MethodInterceptionAspect
	{
		public static AssignExecutionContextAspect Instance { get; } = new AssignExecutionContextAspect();

		AssignExecutionContextAspect() {}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( args.Method ) )
			{
				var output = args.Instance.AsTo<IValue<ITestOutputHelper>, Action<string>>( value => value.Item.WriteLine ) ?? IgnoredOutputCommand.Instance.Run;
				var history = Services.Get<ILoggerHistory>();
				var logger = Services.Get<ILogger>();
				using ( new Diagnostics.ProfilerFactory( output, logger, history ).Create( args.Method ) )
				{
					args.Proceed();
				}
			}
		}
	}

	public static class MethodBaseExtensions
	{
		public static AssignExecutionContextCommand Assign( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => Assign( @this, new RecordingLoggerFactory( history, level ) );

		public static AssignExecutionContextCommand Assign( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new AssignExecutionContextCommand().ExecuteWith( @this );
			DefaultServiceProvider.Instance.Assign( new ServiceProvider( factory ) );
			return result;
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<MethodBase>
	{
		readonly Action<Assembly> initialize;

		public AssignExecutionContextCommand() : this( AssemblyInitializer.Instance.Run, ExecutionContext.Instance ) {}

		public AssignExecutionContextCommand( Action<Assembly> initialize, IWritableValue<MethodBase> value ) : base( value )
		{
			this.initialize = initialize;
		}

		protected override void OnExecute( MethodBase parameter )
		{
			initialize( parameter.DeclaringType.Assembly );

			base.OnExecute( parameter );
		}
	}

	[Disposable]
	public abstract class TestCollectionBase : FixedValue<ITestOutputHelper>
	{
		protected TestCollectionBase( ITestOutputHelper output )
		{
			Assign( output );
		}

		protected ITestOutputHelper Output => Item;

		protected virtual void Dispose( bool disposing ) {}
	}

	public static class Traits
	{
		public const string Category = "Category";

		public static class Categories
		{
			public const string FileSystem = "FileSystem", IoC = "IoC";
		}
	}
}