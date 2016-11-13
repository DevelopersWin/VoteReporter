using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class TypeAspects<T> : Aspects where T : ITypeLevelAspect
	{
		public TypeAspects( ISpecification<TypeInfo> specification ) 
			: this( specification, TypeAspectFactory<T>.Default ) {}

		public TypeAspects( ISpecification<TypeInfo> specification, ISpecificationParameterizedSource<TypeInfo, AspectInstance> source ) 
			: base( specification.And( source ).ToDelegate(), source.Get ) {}
	}
}