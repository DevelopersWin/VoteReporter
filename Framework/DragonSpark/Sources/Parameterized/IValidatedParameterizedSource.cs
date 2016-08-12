namespace DragonSpark.Sources.Parameterized
{
	public interface IValidatedParameterizedSource : IParameterizedSource
	{
		bool IsValid( object parameter );
	}

	public interface IValidatedParameterizedSource<in TParameter, out TResult> : IParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource
	{
		bool IsValid( TParameter parameter );
	}
}