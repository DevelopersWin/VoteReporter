using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public interface IValidatedParameterizedSource : IParameterizedSource, ISpecification
	{
		bool IsValid( object parameter );
	}

	public interface IValidatedParameterizedSource<in TParameter, out TResult> : IParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource, ISpecification<TParameter>
	{
		bool IsValid( TParameter parameter );
	}
}