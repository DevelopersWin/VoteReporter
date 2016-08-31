using System;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class CoercedParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly Coerce<TParameter> coercer;

		public CoercedParameterizedSource( Coerce<TParameter> coercer, Func<TParameter, TResult> source ) : base( source )
		{
			this.coercer = coercer;
		}

		protected override object GetGeneralized( object parameter ) => Get( coercer( parameter ) );
	}

	public sealed class ParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly IParameterizedSource source;

		public ParameterizedSource( IParameterizedSource source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => (TResult)source.Get( parameter );
	}
}