using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class ParameterizedItemSourceBase<TParameter, TItem> : ParameterizedSourceBase<TParameter, ImmutableArray<TItem>>
	{
		public override ImmutableArray<TItem> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		protected abstract IEnumerable<TItem> Yield( TParameter parameter );
	}
}
