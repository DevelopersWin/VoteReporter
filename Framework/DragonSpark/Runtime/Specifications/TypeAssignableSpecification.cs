using DragonSpark.Extensions;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class GenericTypeAssignableSpecification : SpecificationWithContextBase<Type>
	{
		public GenericTypeAssignableSpecification( Type context ) : base( context ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Context.Adapt().IsGenericOf( parameter );
	}

	public class TypeAssignableSpecification<T> : TypeAssignableSpecification
	{
		public static TypeAssignableSpecification<T> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( typeof(T) ) {}
	}

	public class TypeAssignableSpecification : SpecificationWithContextBase<Type>
	{
		public TypeAssignableSpecification( Type targetType ) : base( targetType ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Context.Adapt().IsAssignableFrom( parameter );
	}
}