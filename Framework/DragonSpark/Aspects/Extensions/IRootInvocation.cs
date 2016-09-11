using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensions
{
	public interface IRootInvocation : IAssignableSource<IInvocation>, IAssignable<AspectInvocation>, IInvocation, IComposable<ISpecification<object>>, ISpecification<object> {}
}