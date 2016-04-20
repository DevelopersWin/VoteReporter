using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger.Categories;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
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
				using ( new Diagnostics.ProfilerFactory<Debug>( output, logger, history ).Create( args.Method ) )
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
		public AssignExecutionContextCommand() : this( ExecutionContext.Instance ) {}

		public AssignExecutionContextCommand( IWritableValue<MethodBase> value ) : base( value ) {}
	}

	[Disposable]
	public abstract class TestBase : FixedValue<ITestOutputHelper>
	{
		protected TestBase( ITestOutputHelper output )
		{
			Assign( output );
		}

		protected ITestOutputHelper Output => Item;

		protected virtual void Dispose( bool disposing ) {}
	}
}