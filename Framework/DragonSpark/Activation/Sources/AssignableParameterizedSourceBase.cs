namespace DragonSpark.Activation.Sources
{
	public abstract class AssignableParameterizedSourceBase<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>, IAssignableParameterizedSource<TParameter, TResult>
	{
		public abstract void Set( TParameter parameter, TResult result );
	}
}