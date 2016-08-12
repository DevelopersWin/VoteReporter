namespace DragonSpark.Activation.Sources
{
	public class DecoratedAssignableParameterizedSource<TParameter, TResult> : DecoratedParameterizedSource<TParameter, TResult>, IAssignableParameterizedSource<TParameter, TResult>
	{
		readonly IAssignableParameterizedSource<TParameter, TResult> source;
		public DecoratedAssignableParameterizedSource( IAssignableParameterizedSource<TParameter, TResult> source ) : base( source )
		{
			this.source = source;
		}

		public void Set( TParameter parameter, TResult result ) => source.Set( parameter, result );
	}
}