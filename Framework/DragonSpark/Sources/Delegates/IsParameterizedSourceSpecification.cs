using System;
using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Delegates
{
	public sealed class IsParameterizedSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Default { get; } = new IsParameterizedSourceSpecification().ToCachedSpecification();
		IsParameterizedSourceSpecification() : base( typeof(IParameterizedSource<,>), typeof(IParameterizedSource) ) {}

		public override bool IsSatisfiedBy( Type parameter ) => Adapters.Select( adapter => adapter.Type ).Any( parameter.Adapt().IsGenericOf );
	}
}