using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;
using System.Collections.Generic;

namespace DragonSpark.Activation.IoC
{
	public class BuildKeyMonitorExtension : BuilderStrategy, IRequiresRecovery
	{
		static Stack<NamedTypeBuildKey> Stack => new ThreadAmbientChain<NamedTypeBuildKey>().Value;

		public override void PreBuildUp( IBuilderContext context )
		{
			var stack = Stack;
			if ( stack.Contains( context.BuildKey ) )
			{
				context.BuildComplete = true;
				context.Existing = null;
			}
			else
			{
				context.RecoveryStack.Add( this );
				stack.Push( context.BuildKey );
			}
		}

		public override void PostBuildUp( IBuilderContext context ) => Recover();

		public void Recover() => Stack.Pop();
	}
}