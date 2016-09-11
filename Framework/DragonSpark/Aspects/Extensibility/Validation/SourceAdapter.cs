using System;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class SourceAdapter<TParameter, TResult> : ParameterValidationAdapterBase<TParameter>
	{
		readonly static Func<MethodInfo, bool> Method = MethodEqualitySpecification.For( typeof(IParameterizedSource<TParameter, TResult>).GetTypeInfo().GetDeclaredMethod( nameof(ISource.Get) ) );

		public SourceAdapter( ISpecification<TParameter> inner ) : base( inner, Method ) {}
	}
}