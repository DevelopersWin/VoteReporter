using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase<T> : ValidatedParameterizedSourceBase<T, object>, IActivator where T : TypeRequest
	{
		protected new static ISpecification<object> DefaultSpecification { get; } = IsInstanceOfSpecification<T>.Default.Or( IsInstanceOfSpecification<Type>.Default );

		readonly Coerce<T> coercer;

		protected ActivatorBase( Coerce<T> coercer ) : this( coercer, DefaultSpecification ) {}

		protected ActivatorBase( Coerce<T> coercer, ISpecification<T> specification ) : base( coercer, specification )
		{
			this.coercer = coercer;
		}

		bool ISpecification<TypeRequest>.IsSatisfiedBy( TypeRequest parameter ) => base.IsSatisfiedBy( (T)parameter );

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( (T)parameter );

		object IParameterizedSource<Type, object>.Get( Type parameter ) => Get( coercer.Invoke( parameter ) );

		bool ISpecification<Type>.IsSatisfiedBy( Type parameter ) => base.IsSatisfiedBy( coercer.Invoke( parameter ) );
	}
}