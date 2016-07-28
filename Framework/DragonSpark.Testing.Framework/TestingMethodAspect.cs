using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Aspects;
using System;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Framework
{
	[Serializable, LinesOfCodeAvoided( 8 )]
	public class TestingMethodAspect : MethodInterceptionAspect
	{
		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			base.OnInvoke( args );

			foreach ( var disposable in new IDisposable[] { ExecutionContextStore.Instance.Value, ApplicationServices.Instance.Value }.WhereAssigned() )
			{
				disposable.Dispose();
			}

			// new ApplicationOutputCommand().Execute( new OutputCommand.Parameter( args.Instance, args.Method, args.Proceed ) );

			// args.ReturnValue = Defer.Run( ExecutionContextStore.Instance.Value.Dispose, args.ReturnValue );
		}
	}
}