using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation
{
	public interface IActivator : IParameterizedSource<Type, object>, IServiceProvider, ISpecification<Type> {}
}