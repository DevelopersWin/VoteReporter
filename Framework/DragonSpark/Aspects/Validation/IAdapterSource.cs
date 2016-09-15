using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public interface IAdapterSource : IParameterizedSource<IParameterValidationAdapter>, ISpecification<Type> {}
}