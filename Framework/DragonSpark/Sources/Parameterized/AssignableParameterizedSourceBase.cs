namespace DragonSpark.Sources.Parameterized
{
	public abstract class AssignableReferenceSourceBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAssignableReferenceSource<TParameter, TResult>
	{
		public abstract void Set( TParameter parameter, TResult result );
	}
}