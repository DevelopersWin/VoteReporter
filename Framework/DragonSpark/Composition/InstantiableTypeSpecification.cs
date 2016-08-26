using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Composition
{
	public sealed class InstantiableTypeSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new InstantiableTypeSpecification().ToCachedSpecification();
		InstantiableTypeSpecification() : this( typeof(Delegate), typeof(Array) ) {}

		readonly ImmutableArray<TypeAdapter> exempt;

		public InstantiableTypeSpecification( params Type[] exempt )
		{
			this.exempt = exempt.Select( type => type.Adapt() ).ToImmutableArray();
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter != typeof(object) && !exempt.IsAssignableFrom( parameter );
	}
}