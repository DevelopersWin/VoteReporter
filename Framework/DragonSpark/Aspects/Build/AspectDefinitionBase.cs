using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Build
{
	public abstract class AspectDefinitionBase : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectDefinition
	{
		protected AspectDefinitionBase( Func<TypeInfo, bool> specification, Func<TypeInfo, AspectInstance> factory ) : base( specification, factory ) {}
	}
}