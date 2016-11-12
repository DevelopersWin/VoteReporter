using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Specifications
{
	public class AdapterAssignableSpecification : DelegatedSpecification<Type>, ISpecification<TypeInfo>
	{
		public AdapterAssignableSpecification( params Type[] types ) : base( types.IsAssignableFrom ) {}
		public AdapterAssignableSpecification( params TypeAdapter[] types ) : base( types.IsAssignableFrom ) {}

		public bool IsSatisfiedBy( TypeInfo parameter ) => IsSatisfiedBy( parameter.AsType() );
	}
}