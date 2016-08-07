using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Aspects;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public sealed class TestingMethodAspect : MethodInterceptionAspect
	{
		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			base.OnInvoke( args );

			// new ApplicationOutputCommand().Execute( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );

			var disposable = (IDisposable)ApplicationServices.Instance.Get() ?? ExecutionContext.Instance.Get();
			args.ReturnValue = Defer.Run( disposable.Dispose, args.ReturnValue );
		}
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

		public static void Dispose<T>( this IDisposable @this, T item ) => @this.Dispose(); // TODO: Make more generalized.
	}
}