using System.Collections.Immutable;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Specifications;

namespace DragonSpark.Activation.Location
{
	public class SingletonSpecification : SpecificationBase<SingletonRequest>
	{
		public static SingletonSpecification Default { get; } = new SingletonSpecification();
		SingletonSpecification() : this( "Instance", "Default" ) {}

		readonly ImmutableArray<string> candidates;

		public SingletonSpecification( params string[] candidates ) : this( candidates.ToImmutableArray() ) {}

		public SingletonSpecification( ImmutableArray<string> candidates )
		{
			this.candidates = candidates;
		}

		public override bool IsSatisfiedBy( SingletonRequest parameter )
		{
			var result =
				SourceTypeAssignableSpecification.Default.IsSatisfiedBy( new SourceTypeAssignableSpecification.Parameter( parameter.RequestedType, parameter.Candidate.PropertyType ) )
				&& 
				parameter.Candidate.GetMethod.IsStatic && !parameter.Candidate.GetMethod.ContainsGenericParameters 
				&& 
				( candidates.Contains( parameter.Candidate.Name ) || parameter.Candidate.Has<SingletonAttribute>() );
			return result;
		}
	}
}