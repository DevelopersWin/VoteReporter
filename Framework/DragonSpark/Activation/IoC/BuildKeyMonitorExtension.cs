using System.Collections.Generic;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;

namespace DragonSpark.Activation.IoC
{
	public class ClearTrackingDataExtension : BuilderStrategy
	{
		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !new Checked( context.BuildKey.Type, this ).Item.IsApplied )
			{
				
			}
			base.PreBuildUp( context );
		}
	}

	public class BuildKeyMonitorExtension : BuilderStrategy, IRequiresRecovery
	{
		static Stack<NamedTypeBuildKey> Stack => new ThreadAmbientChain<NamedTypeBuildKey>().Item;

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