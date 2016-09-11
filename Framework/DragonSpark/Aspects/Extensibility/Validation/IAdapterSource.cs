using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public interface IAdapterSource : IParameterizedSource<IParameterValidationAdapter>, ISpecification<Type> {}
}