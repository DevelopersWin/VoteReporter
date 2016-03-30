using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;

namespace DragonSpark.Activation.IoC
{
	public class BuildKeyMonitorExtension : BuilderStrategy
	{
		public override void PreBuildUp( IBuilderContext context )
		{
			var item = new ThreadAmbientChain<NamedTypeBuildKey>().Item;
			if ( item.Contains( context.BuildKey ) )
			{
				context.BuildComplete = true;
				context.Existing = null;
			}
			else
			{
				item.Push( context.BuildKey );
			}
		}

		public override void PostBuildUp( IBuilderContext context ) => new ThreadAmbientChain<NamedTypeBuildKey>().Item.Pop();
	}
}