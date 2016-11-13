using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class TypeAspectDefinition<T> : AspectDefinition where T : ITypeLevelAspect
	{
		public TypeAspectDefinition( ISpecification<TypeInfo> specification ) 
			: this( specification, TypeAspectFactory<T>.Default ) {}

		public TypeAspectDefinition( ISpecification<TypeInfo> specification, ISpecificationParameterizedSource<TypeInfo, AspectInstance> source ) 
			: base( specification.And( source ).ToDelegate(), source.Get ) {}
	}
}