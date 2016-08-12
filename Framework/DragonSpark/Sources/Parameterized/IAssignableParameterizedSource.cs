namespace DragonSpark.Sources.Parameterized
{
	public interface IAssignableParameterizedSource<T> : IAssignableParameterizedSource<object, T>, IParameterizedSource<T> {}
	public interface IAssignableParameterizedSource<in TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		void Set( TParameter parameter, TResult result );
	}
}