using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation.IoC
{
	interface IResolutionSupport : ISpecification<TypeRequest>
	{
		// bool CanResolve( TypeRequest request );
	}
}