namespace DragonSpark.Sources.Parameterized
{
	public interface IAssignableReferenceSource<T> : IAssignableReferenceSource<object, T>, IParameterizedSource<T> {}
	public interface IAssignableReferenceSource<in TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		void Set( TParameter parameter, TResult result );
	}
}