namespace DragonSpark.Sources.Parameterized
{
	public class DecoratedParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly IParameterizedSource<TParameter, TResult> source;

		public DecoratedParameterizedSource( IParameterizedSource<TParameter, TResult> source ) : base( source.Get )
		{
			this.source = source;
		}

		protected override object GetGeneralized( object parameter ) => source.Get( parameter );
	}
}