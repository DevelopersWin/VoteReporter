using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), 
		AttributeUsage( AttributeTargets.Method )/*, 
		MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )*/
		]
	public abstract class AspectBase : MethodInterceptionAspect {}
}