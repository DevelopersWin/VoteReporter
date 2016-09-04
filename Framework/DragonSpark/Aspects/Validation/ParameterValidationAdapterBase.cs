using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public abstract class ParameterValidationAdapterBase<T> : DecoratedSpecification<T>, IParameterValidationAdapter
	{
		readonly Func<MethodInfo, bool> method;

		protected ParameterValidationAdapterBase( ISpecification<T> inner, MethodInfo method ) : this( inner, MethodEqualitySpecification.For( method ) ) {}

		ParameterValidationAdapterBase( ISpecification<T> inner, Func<MethodInfo, bool> method ) : base( inner )
		{
			this.method = method;
		}

		public bool IsSatisfiedBy( MethodInfo parameter ) => method( parameter );

		public virtual bool IsSatisfiedBy( object parameter ) => parameter is T && base.IsSatisfiedBy( (T)parameter );
	}
}