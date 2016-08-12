using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;
using Microsoft.Practices.ObjectBuilder2;

namespace DragonSpark.Activation.IoC
{
	public class BuildKeyMonitorExtension : BuilderStrategy, IRequiresRecovery
	{
		static IStack<NamedTypeBuildKey> Stack => AmbientStack<NamedTypeBuildKey>.Default.Get();

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