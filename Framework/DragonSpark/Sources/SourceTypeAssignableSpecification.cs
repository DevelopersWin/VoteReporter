using DragonSpark.Activation;
using DragonSpark.Sources.Coercion;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Sources
{
	public sealed class SourceTypeAssignableSpecification : SpecificationBase<SourceTypeCandidateParameter>
	{
		public static SourceTypeAssignableSpecification Default { get; } = new SourceTypeAssignableSpecification();
		SourceTypeAssignableSpecification() : this( SourceAccountedTypes.Default.To( ParameterConstructor<ImmutableArray<Type>, CompositeAssignableSpecification>.Default ).Get ) {}

		readonly Func<Type, ISpecification<Type>> specificationSource;

		[UsedImplicitly]
		public SourceTypeAssignableSpecification( Func<Type, ISpecification<Type>> specificationSource )
		{
			this.specificationSource = specificationSource;
		}

		public override bool IsSatisfiedBy( SourceTypeCandidateParameter parameter ) => 
			specificationSource( parameter.Candidate ).IsSatisfiedBy( parameter.TargetType );
	}
}