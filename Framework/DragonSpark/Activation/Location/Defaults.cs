using DragonSpark.Sources;
using DragonSpark.Specifications;
using System.Reflection;

namespace DragonSpark.Activation.Location
{
	public static class Defaults
	{
		public static ISpecification<SingletonRequest> SourcedSingleton { get; } = SingletonSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ).And( SourceTypeAssignableSpecification.Default.Project<SingletonRequest, SourceTypeAssignableSpecification.Parameter>( destination => new SourceTypeAssignableSpecification.Parameter( destination.RequestedType, destination.Candidate.PropertyType ) ) );
	}
}