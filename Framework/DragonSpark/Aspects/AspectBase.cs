using System;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	public abstract class AspectBase : MethodInterceptionAspect {}
}