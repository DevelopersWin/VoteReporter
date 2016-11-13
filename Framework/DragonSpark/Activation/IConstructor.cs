using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Activation
{
	public interface IConstructor : IParameterizedSource<ConstructTypeRequest, object>, ISpecification<ConstructTypeRequest>, IActivator {}
}