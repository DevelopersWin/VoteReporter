using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TParameter, TResult> : FactoryBase<TParameter, TResult>, IActivator where TParameter : TypeRequest where TResult : class
	{
		protected ActivatorBase( ISpecification<TParameter> specification, IFactoryParameterCoercer<TParameter> coercer ) : base( specification, coercer ) {}

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => CreateFromItem( parameter );
	}

	/*public abstract class ActivatorBase : ActivatorBase<object>
	{
		protected ActivatorBase( ISpecification<LocateTypeRequest> specification ) : base( specification ) {}
	}*/

	public abstract class LocatorBase : ActivatorBase<LocateTypeRequest, object>, ILocator
	{
		protected LocatorBase() : this( AlwaysSpecification.Instance.Wrap<LocateTypeRequest>() ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( specification, ActivateRequestCoercer<object>.Instance ) {}

		// protected override TResult Activate( LocateTypeRequest parameter ) => Activator.Activate<TResult>( parameter.Type, parameter.Name );
		// object IFactory<LocateTypeRequest, object>.Create( LocateTypeRequest parameter ) => Create( parameter );
	}
}