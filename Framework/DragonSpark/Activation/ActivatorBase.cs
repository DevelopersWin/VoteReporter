using System;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TRequest> : FactoryBase<TRequest, object>, IActivator where TRequest : TypeRequest
	{
		protected ActivatorBase( IParameterCoercer<TRequest> coercer ) : this( IsTypeSpecification<TRequest>.Instance, coercer ) {}

		protected ActivatorBase( ISpecification<TRequest> specification, IParameterCoercer<TRequest> coercer ) : base( specification, coercer ) {}

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => CreateFromItem( parameter );
	}

	public abstract class LocatorBase : LocatorBase<object>
	{
		protected LocatorBase() {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( specification ) {}
	}

	public abstract class LocatorBase<T> : ActivatorBase<LocateTypeRequest> where T : class
	{
		protected LocatorBase() : base( Coercer.Instance ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( specification, Coercer.Instance ) {}

		public class Coercer : TypeRequestCoercer<LocateTypeRequest>
		{
			public new static Coercer Instance { get; } = new Coercer();
		
			protected override LocateTypeRequest Create( Type type ) => new LocateTypeRequest( type );
		}
	}
}