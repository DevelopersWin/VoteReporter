using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstances<T> : AspectInstances where T : IAspect
	{
		public static AspectInstances<T> Default { get; } = new AspectInstances<T>();
		AspectInstances() : base( ObjectConstructionFactory<T>.Default.Get() ) {}

		/*public AspectInstances( ObjectConstruction construction ) : base( construction ) {}*/
	}
}