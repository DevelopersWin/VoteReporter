using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation
{
	public abstract class ConstructorBase : ActivatorBase<ConstructTypeRequest, object>
	{
		protected ConstructorBase() : this( AlwaysSpecification.Instance.Wrap<ConstructTypeRequest>() ) {}

		protected ConstructorBase( ISpecification<ConstructTypeRequest> specification  ) : base( specification, ConstructorParameterCoercer<object>.Instance ) {}
	}
}