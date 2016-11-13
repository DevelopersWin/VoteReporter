using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class AspectDefinition : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspectDefinition
	{
		public AspectDefinition( ISpecificationParameterizedSource<TypeInfo, AspectInstance> source ) : this( source.IsSatisfiedBy, source.Get ) {}

		protected AspectDefinition( Func<TypeInfo, bool> specification, Func<TypeInfo, AspectInstance> factory ) : base( specification, factory ) {}
	}
}