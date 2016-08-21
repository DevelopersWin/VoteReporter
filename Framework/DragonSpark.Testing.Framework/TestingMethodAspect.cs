using DragonSpark.Aspects;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using PostSharp.Aspects;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Framework
{
	[LinesOfCodeAvoided( 8 )]
	public sealed class TestingMethodAspect : TimeAttributeBase
	{
		public TestingMethodAspect() : base( "Executing Test Method '{@Method}'" ) {}

		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( new PurgingContext() )
			{
				base.OnInvoke( args );
			}
			
			var disposable = (IDisposable)ApplicationServices.Instance.Get() ?? ExecutionContext.Instance.Get();
			args.ReturnValue = Defer.Run( new Action( disposable.Dispose ).Wrap<Task>(), args.ReturnValue );
		}
	}

	public sealed class PurgingContext : InitializedDisposableAction
	{
		public PurgingContext() : base( PurgeLoggerMessageHistoryCommand.Defaults.Get().Fixed( Output.Instance.Get() ).Run ) {}
	}

	public static class Defer
	{
		public static Task Run( Action<Task> action, object context )
		{
			var task = context as Task;
			if ( task != null )
			{
				return task.ContinueWith( action );
			}
			action( Task.CompletedTask );
			return null;
		}
	}
}