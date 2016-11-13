using System;
using System.Collections.Generic;

namespace DragonSpark.Sources.Parameterized
{
	public class DelegatedParameterizedItemSource<TParameter, TItem> : ParameterizedItemSourceBase<TParameter, TItem>
	{
		readonly Func<TParameter, IEnumerable<TItem>> factory;
		public DelegatedParameterizedItemSource( Func<TParameter, IEnumerable<TItem>> factory )
		{
			this.factory = factory;
		}

		public override IEnumerable<TItem> Yield( TParameter parameter ) => factory( parameter );
	}
}