using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TRequest> : ValidatedParameterizedSourceBase<TRequest, object>, IActivator where TRequest : TypeRequest
	{
		readonly static ISpecification<object> Specification = IsInstanceOfSpecification<TRequest>.Instance.Or( IsInstanceOfSpecification<Type>.Instance );

		protected ActivatorBase( Coerce<TRequest> coercer ) : this( coercer, Specification ) {}

		protected ActivatorBase( Coerce<TRequest> coercer, ISpecification<TRequest> specification ) : base( coercer, specification ) {}

		bool ISpecification<TypeRequest>.IsSatisfiedBy( TypeRequest parameter ) => base.IsSatisfiedBy( (TRequest)parameter );

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( (TRequest)parameter );
		//public bool IsSatisfiedBy( TypeRequest parameter ) => base.IsSatisfiedBy( (TRequest)parameter );
	}

	public abstract class LocatorBase : ActivatorBase<LocateTypeRequest>
	{
		readonly protected static Coerce<LocateTypeRequest> DefaultCoerce = Coercer.Instance.ToDelegate();

		protected LocatorBase() : base( DefaultCoerce ) {}

		protected LocatorBase( ISpecification<LocateTypeRequest> specification ) : base( DefaultCoerce, specification ) {}

		public sealed class Coercer : TypeRequestCoercer<LocateTypeRequest>
		{
			public static Coercer Instance { get; } = new Coercer();
		
			protected override LocateTypeRequest Create( Type type ) => new LocateTypeRequest( type );
		}
	}
}