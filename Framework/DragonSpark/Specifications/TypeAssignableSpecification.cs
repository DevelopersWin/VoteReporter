using System;
using DragonSpark.Extensions;

namespace DragonSpark.Specifications
{
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