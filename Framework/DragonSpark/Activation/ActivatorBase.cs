using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<TRequest> : ValidatedParameterizedSourceBase<TRequest, object>, IActivator where TRequest : TypeRequest
	{
		readonly protected static ISpecification<object> DefaultSpecification = IsInstanceOfSpecification<TRequest>.Default.Or( IsInstanceOfSpecification<Type>.Default );

		readonly Coerce<TRequest> coercer;

		protected ActivatorBase( Coerce<TRequest> coercer ) : this( coercer, DefaultSpecification ) {}

		protected ActivatorBase( Coerce<TRequest> coercer, ISpecification<TRequest> specification ) : base( coercer, specification )
		{
			this.coercer = coercer;
		}

		bool ISpecification<TypeRequest>.IsSatisfiedBy( TypeRequest parameter ) => base.IsSatisfiedBy( (TRequest)parameter );

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( (TRequest)parameter );

		object IParameterizedSource<Type, object>.Get( Type parameter ) => Get( coercer.Invoke( parameter ) );

		bool ISpecification<Type>.IsSatisfiedBy( Type parameter ) => base.IsSatisfiedBy( coercer.Invoke( parameter ) );
	}
}