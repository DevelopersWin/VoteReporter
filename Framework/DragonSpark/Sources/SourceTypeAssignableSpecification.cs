using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Specifications;

namespace DragonSpark.Sources
{
	public sealed class SourceTypeAssignableSpecification : SpecificationBase<SourceTypeAssignableSpecification.Parameter>
	{
		public static SourceTypeAssignableSpecification Default { get; } = new SourceTypeAssignableSpecification();
		SourceTypeAssignableSpecification() {}

		public override bool IsSatisfiedBy( Parameter parameter )
		{
			foreach ( var candidate in Candidates( parameter.Candidate ) )
			{
				if ( candidate.Adapt().IsAssignableFrom( parameter.TargetType ) )
				{
					return true;
				}
			}
			return false;
		}

		static IEnumerable<Type> Candidates( Type type )
		{
			yield return type;
			var implementations = type.Adapt().GetImplementations( typeof(ISource<>) );
			if ( implementations.Any() )
			{
				yield return implementations.First().Adapt().GetInnerType();
			}
		}

		public struct Parameter
		{
			public Parameter( Type targetType, Type candidate )
			{
				TargetType = targetType;
				Candidate = candidate;
			}

			public Type TargetType { get; }
			public Type Candidate { get; }
		}
	}
}