namespace DragonSpark.Sources.Parameterized
{
	public abstract class ParameterizedSourceBase<T> : ParameterizedSourceBase<object, T>, IParameterizedSource<T> {}

	public abstract class ParameterizedSourceBase<TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		/*readonly Coerce<TParameter> coercer;

		protected ParameterizedSourceBase() : this( Defaults<TParameter>.Coercer ) {}

		protected ParameterizedSourceBase( Coerce<TParameter> coercer )
		{
			this.coercer = coercer;
		}*/

		public abstract TResult Get( TParameter parameter );

		/*object IParameterizedSource.Get( object parameter ) => GetGeneralized( parameter );

		protected virtual object GetGeneralized( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssignedOrValue() ? Get( coerced ) : default(TResult);
			return result;
		}*/
	}
}