using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

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

		protected override bool Coerce( [Optional]object parameter ) => parameter is MethodInfo ? IsSatisfiedBy( (MethodInfo)parameter ) : base.Coerce( parameter );
	}
}