using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Testing.Framework.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Framework
{
	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 4 )]
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public sealed class TestingMethodAspect : MethodInterceptionAspect
	{
		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( CurrentTestingMethod.Default.Get() == null )
			{
				CurrentTestingMethod.Default.Assign( args.Method );

				using ( new MethodOperationContext( args.Method ) )
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

	/*public sealed class TimedAttribute : Aspects.TimedAttribute
	{
		public TimedAttribute() : this( "Executing Test Method '{@Method}'" ) {}
		public TimedAttribute( string template ) : base( template ) {}

		public override bool CompileTimeValidate( MethodBase method )
		{
			return base.CompileTimeValidate( method );
		}
	}*/

	public sealed class CurrentTestingMethod : Scope<MethodBase>
	{
		public static CurrentTestingMethod Default { get; } = new CurrentTestingMethod();
		CurrentTestingMethod() {}
	}
}