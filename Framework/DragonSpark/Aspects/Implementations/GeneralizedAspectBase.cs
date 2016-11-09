using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public abstract class GeneralizedAspectBase : InvocationAspectBase
	{
		protected GeneralizedAspectBase() : this( o => default(IAspect) ) {}

		protected GeneralizedAspectBase( Func<object, IAspect> instanceFactory ) : base( instanceFactory, Definition.Default ) {}
	}
}