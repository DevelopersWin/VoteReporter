using System;
using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public abstract class ParameterValidationAdapterBase<T> : IParameterValidationAdapter
	{
		readonly ISpecification<T> inner;
		readonly Func<MethodInfo, bool> method;

		protected ParameterValidationAdapterBase( ISpecification<T> inner, Func<MethodInfo, bool> method )
		{
			this.inner = inner;
			this.method = method;
		}

		public bool IsSatisfiedBy( MethodInfo parameter ) => method( parameter );

		public virtual bool IsSatisfiedBy( object parameter ) => parameter is T && inner.IsSatisfiedBy( (T)parameter );
	}
}