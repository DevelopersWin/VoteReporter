using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Activation.Location
{
	public static class Defaults
	{
		public static ISpecification<SingletonRequest> SourcedSingleton { get; } = SingletonSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ).And( SourceTypeAssignableSpecification.Default.Project<SingletonRequest, SourceTypeAssignableSpecification.Parameter>( destination => new SourceTypeAssignableSpecification.Parameter( destination.RequestedType, destination.Candidate.PropertyType ) ) );
	}

	public class SingletonProperties : ParameterizedSourceBase<Type, PropertyInfo>
	{
		public static IParameterizedSource<Type, PropertyInfo> Default { get; } = new SingletonProperties().ToCache();
		SingletonProperties() : this( Defaults.SourcedSingleton ) {}

		readonly ISpecification<SingletonRequest> specification;

		public SingletonProperties( ISpecification<SingletonRequest> specification )
		{
			this.specification = specification;
		}

		public override PropertyInfo Get( Type parameter )
		{
			foreach ( var property in parameter.GetTypeInfo().DeclaredProperties.Fixed() )
			{
				if ( specification.IsSatisfiedBy( new SingletonRequest( parameter, property ) ) )
				{
					return property;
				}
			}
			return null;
		}
	}
}