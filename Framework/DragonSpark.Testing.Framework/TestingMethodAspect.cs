using DragonSpark.Application;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Framework
{
	[LinesOfCodeAvoided( 8 )]
	public sealed class TestingMethodAspect : TimedAttributeBase
	{
		public TestingMethodAspect() : base( "Executing Test Method '{@Method}'" ) {}

		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( CurrentTestingMethod.Default.Get() == null )
			{
				CurrentTestingMethod.Default.Assign( args.Method );

				using ( new PurgingContext() )
				{
					base.OnInvoke( args );
				}
			
				var disposable = (IDisposable)ApplicationServices.Default.Get() ?? ExecutionContext.Default.Get();
				args.ReturnValue = Defer.Run( new Action( disposable.Dispose ).Wrap<Task>(), args.ReturnValue );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	sealed class CurrentTestingMethod : Scope<MethodBase>
	{
		public static CurrentTestingMethod Default { get; } = new CurrentTestingMethod();
		CurrentTestingMethod() {}
	}
}