using System.Reflection;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	public class FactoryAdapter : ParameterValidationAdapterBase<object>
	{
		readonly static MethodInfo Method = typeof(IParameterizedSource).GetTypeInfo().GetDeclaredMethod( nameof(IParameterizedSource.Get) );

		public FactoryAdapter( ISpecification inner ) : base( new DelegatedSpecification<object>( inner.IsSatisfiedBy ), Method ) {}
	}
}