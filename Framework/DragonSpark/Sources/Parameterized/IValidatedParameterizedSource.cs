using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public interface IValidatedParameterizedSource : IParameterizedSource, ISpecification {}

	public interface IValidatedParameterizedSource<in TParameter, out TResult> : IParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource, ISpecification<TParameter> {}
}