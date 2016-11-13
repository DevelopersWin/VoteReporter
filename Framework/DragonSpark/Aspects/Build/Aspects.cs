using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class Aspects : SpecificationParameterizedSource<TypeInfo, AspectInstance>, IAspects
	{
		public Aspects( ISpecificationParameterizedSource<TypeInfo, AspectInstance> source ) : this( source.IsSatisfiedBy, source.Get ) {}

		protected Aspects( Func<TypeInfo, bool> specification, Func<TypeInfo, AspectInstance> factory ) : base( specification, factory ) {}
	}
}