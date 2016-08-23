using System;
using DragonSpark.Extensions;

namespace DragonSpark.Specifications
{
	public class GenericTypeAssignableSpecification : DelegatedSpecification<Type>
	{
		public GenericTypeAssignableSpecification( Type context ) : base( context.Adapt().IsGenericOf ) {}
	}
}