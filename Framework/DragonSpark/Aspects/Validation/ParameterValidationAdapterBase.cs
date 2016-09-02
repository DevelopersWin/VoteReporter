using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Validation
{
	public abstract class ParameterValidationAdapterBase<T> : DecoratedSpecification<object>, IParameterValidationAdapter
	{
		readonly Func<MethodInfo, bool> method;

		protected ParameterValidationAdapterBase( ISpecification<T> inner, MethodInfo method ) : this( inner, MethodEqualitySpecification.For( method ) ) {}

		ParameterValidationAdapterBase( ISpecification<T> inner, Func<MethodInfo, bool> method ) : base( inner.With( Coercer<T>.Default ) )
		{
			this.method = method;
		}

		public bool IsSatisfiedBy( MethodInfo parameter ) => method( parameter );

		public override bool IsSatisfiedBy( [Optional] object parameter ) => parameter is MethodInfo ? IsSatisfiedBy( (MethodInfo)parameter ) : base.IsSatisfiedBy( parameter );

		// public bool IsSatisfiedBy( [Optional]object parameter ) => ;
	}
}