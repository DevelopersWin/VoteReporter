namespace DragonSpark.Sources.Parameterized
{
	public class DecoratedAssignableReferenceSource<TParameter, TResult> : DecoratedParameterizedSource<TParameter, TResult>, IAssignableReferenceSource<TParameter, TResult>
	{
		readonly IAssignableReferenceSource<TParameter, TResult> source;
		public DecoratedAssignableReferenceSource( IAssignableReferenceSource<TParameter, TResult> source ) : base( source )
		{
			this.source = source;
		}

		public void Set( TParameter parameter, TResult result ) => source.Set( parameter, result );
	}
}