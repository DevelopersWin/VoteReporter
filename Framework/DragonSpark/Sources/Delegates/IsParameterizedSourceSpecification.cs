using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Delegates
{
	public sealed class IsParameterizedSourceSpecification : AdapterSpecificationBase
	{
		public static ISpecification<Type> Default { get; } = new IsParameterizedSourceSpecification().ToCachedSpecification();
		IsParameterizedSourceSpecification() : base( typeof(IParameterizedSource<,>) ) {}
	}
}