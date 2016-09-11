using DragonSpark.Sources;

namespace DragonSpark.Aspects.Extensibility
{
	public interface IRootInvocation : IAssignableSource<IInvocation>, IAssignable<AspectInvocation>, IInvocation/*, IComposable<ISpecification<object>>*//*, ISpecification<object>*/ {}
}