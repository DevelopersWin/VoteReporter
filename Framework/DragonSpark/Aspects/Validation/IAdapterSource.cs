using System;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	public interface IAdapterSource : IParameterizedSource<IParameterValidationAdapter>, ISpecification<Type> {}
}