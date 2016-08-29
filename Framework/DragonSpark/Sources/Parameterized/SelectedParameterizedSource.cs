using System;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class SelectedParameterizedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly Transform<TParameter> selector;

		public SelectedParameterizedSource( Transform<TParameter> selector, Func<TParameter, TResult> inner ) : base( inner )
		{
			this.selector = selector;
		}
		
		public override TResult Get( TParameter parameter ) => base.Get( selector( parameter ) );
	}
}