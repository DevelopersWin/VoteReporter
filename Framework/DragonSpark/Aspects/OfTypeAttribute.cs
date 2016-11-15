using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using PostSharp.Reflection;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects
{
	public class OfTypeAttribute : LocationContractAttribute, ILocationValidationAspect<Type>
	{
		readonly ISpecification<Type> types;

		public OfTypeAttribute( params Type[] types ) : this( types.ToImmutableArray() ) {}

		OfTypeAttribute( ImmutableArray<Type> types ) : this( new CompositeAssignableSpecification( types.ToArray() ), $"The specified type is not of type (or cannot be cast to) {string.Join( " or ", types.Select( type => type.FullName ) )}" ) {}

		OfTypeAttribute( ISpecification<Type> types, string errorMessage )
		{
			this.types = types;
			ErrorMessage = errorMessage;
		}

		public Exception ValidateValue( Type value, string locationName, LocationKind locationKind ) => value != null && !types.IsSatisfiedBy( value ) ? CreateArgumentException( value, locationName, locationKind ) : null;
	}
}