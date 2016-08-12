namespace DragonSpark.Sources.Parameterized
{
	public interface IValidatedParameterizedSource<in TParameter, out TResult> : IParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource
	{
		bool IsValid( TParameter parameter );
	}
}