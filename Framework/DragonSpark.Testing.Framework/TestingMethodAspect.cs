using DragonSpark.Application;
using DragonSpark.Commands;
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
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 4 ), ProvideAspectRole( StandardRoles.Tracing ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation ),]
	public sealed class TestingMethodAspect : MethodInterceptionAspect
	{
		public override bool CompileTimeValidate( MethodBase method ) => method.Has<FactAttribute>();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var current = CurrentTestingMethod.Default.Get();
			if ( current == null )
			{
				CurrentTestingMethod.Default.Assign( args.Method );
				try
				{
					using ( new MethodOperationContext( args.Method ) )
					{
						base.OnInvoke( args );
					}
				}
				finally
				{
					var disposable = (IDisposable)ApplicationServices.Default.Get() ?? ExecutionContext.Default.Get();
					args.ReturnValue = Defer.Run( new Action( disposable.Dispose ).Wrap<Task>(), args.ReturnValue );
				}
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}