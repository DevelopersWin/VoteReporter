using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public sealed class ParameterizedSourceAdapter<TParameter, TResult> : ParameterValidationAdapterBase<TParameter>
	{
		readonly static Func<MethodInfo, bool> Method = MethodEqualitySpecification.For( typeof(IParameterizedSource<TParameter, TResult>).GetTypeInfo().GetDeclaredMethod( nameof(ISourceAware.Get) ) );

		public ParameterizedSourceAdapter( ISpecification<TParameter> inner ) : base( inner, Method ) {}
	}
}