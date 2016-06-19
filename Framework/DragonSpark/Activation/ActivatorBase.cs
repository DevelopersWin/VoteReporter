using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TRequest> : FactoryBase<TRequest, object>, IActivator where TRequest : TypeRequest
	{
		protected ActivatorBase( Coerce<TRequest> coercer ) : this( coercer, Specification.Instance ) {}

		protected ActivatorBase( Coerce<TRequest> coercer, ISpecification<TRequest> specification ) : base( coercer, specification ) {}

		bool IFactory<TypeRequest, object>.CanCreate( TypeRequest parameter ) => base.CanCreate( (TRequest)parameter );

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => this.CreateUsing<object>( parameter );

		class Specification : IsInstanceOfSpecification<TRequest>
		{
			public new static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( object parameter ) => base.IsSatisfiedBy( parameter ) || IsInstanceOfSpecification<Type>.Instance.IsSatisfiedBy( parameter );
		}
	}

	/*public abstract class LocatorBase : LocatorBase<object>
	{
		protected LocatorBase() {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( specification ) {}
	}*/

	public abstract class LocatorBase : ActivatorBase<LocateTypeRequest>
	{
		readonly protected static Coerce<LocateTypeRequest> Coerce = Coercer.Instance.ToDelegate();

		protected LocatorBase() : base( Coerce ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( Coerce, specification ) {}

		public class Coercer : TypeRequestCoercer<LocateTypeRequest>
		{
			public static Coercer Instance { get; } = new Coercer();
		
			protected override LocateTypeRequest Create( Type type ) => new LocateTypeRequest( type );
		}
	}
}