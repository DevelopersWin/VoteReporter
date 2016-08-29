using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Composition
{
	sealed class IsConventionCandidateSpecification : SpecificationBase<Type>
	{
		public static IParameterizedSource<Type, Func<Type, bool>> Defaults { get; } = new Cache<Type, Func<Type, bool>>( t => new IsConventionCandidateSpecification( ConventionCandidateNames.Default.Get( t ) ).And( TypeAssignableSpecification.Defaults.Get( t ), Activation.Defaults.Instantiable ).IsSatisfiedBy );
		
		readonly string name;

		public IsConventionCandidateSpecification( string name )
		{
			this.name = name;
		}

		public override bool IsSatisfiedBy( Type parameter ) => parameter.Name.Equals( name );
	}
}