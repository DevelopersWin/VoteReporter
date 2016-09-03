using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public abstract class ParameterValidationAdapterBase<T> : DecoratedSpecification<T>, IParameterValidationAdapter
	{
		readonly ISpecification<object> general;
		readonly Func<MethodInfo, bool> method;

		protected ParameterValidationAdapterBase( ISpecification<T> inner, MethodInfo method ) : this( inner, MethodEqualitySpecification.For( method ), inner as ISpecification<object> ) {}

		ParameterValidationAdapterBase( ISpecification<T> inner, Func<MethodInfo, bool> method, ISpecification<object> general = null ) : base( inner )
		{
			this.general = general;
			this.method = method;
		}

		public bool IsSatisfiedBy( MethodInfo parameter ) => method( parameter );

		public bool IsSatisfiedBy( object parameter ) => parameter is T ? base.IsSatisfiedBy( (T)parameter ) : general != null && general.IsSatisfiedBy( parameter );
	}
}