using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Extensions
{
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationAspectBase : AspectBase {}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	public abstract class AspectBase : MethodInterceptionAspect {}
}