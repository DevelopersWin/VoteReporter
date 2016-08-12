namespace DragonSpark.Sources.Parameterized
{
	public abstract class ParameterizedSourceBase<T> : ParameterizedSourceBase<object, T>, IParameterizedSource<T> {}

	public abstract class ParameterizedSourceBase<TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter ) => parameter is TParameter ? Get( (TParameter)parameter ) : default(TResult);
	}
}