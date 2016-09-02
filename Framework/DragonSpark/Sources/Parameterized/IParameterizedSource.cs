namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedSource
	{
		object Get( object parameter );
	}

	public interface IParameterizedSource<out T> : IParameterizedSource<object, T> {}

	public interface IParameterizedSource<in TParameter, out TResult> // : IParameterizedSource
	{
		TResult Get( TParameter parameter );
	}
}