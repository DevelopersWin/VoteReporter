using DragonSpark.Extensions;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class GenericTypeAssignableSpecification : DelegatedSpecification<Type>
	{
		public GenericTypeAssignableSpecification( Type context ) : base( context.Adapt().IsGenericOf ) {}
	}

	public sealed class TypeAssignableSpecification<T> : TypeAssignableSpecification
	{
		public static TypeAssignableSpecification<T> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( typeof(T) ) {}
	}

	public class TypeAssignableSpecification : DelegatedSpecification<Type>
	{
		public TypeAssignableSpecification( Type targetType ) : base( targetType.Adapt().IsAssignableFrom ) {}
	}
}