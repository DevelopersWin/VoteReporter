using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects.Implementations
{
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), LinesOfCodeAvoided( 1 )]
	public abstract class GeneralizedAspectBase : InvocationAspectBase
	{
		protected GeneralizedAspectBase() : this( o => default(IAspect) ) {}

		protected GeneralizedAspectBase( Func<object, IAspect> instanceFactory ) : base( instanceFactory, Support.Default ) {}
	}
}