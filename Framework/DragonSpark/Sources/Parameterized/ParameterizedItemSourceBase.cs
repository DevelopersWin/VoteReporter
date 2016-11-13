using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class ParameterizedItemSourceBase<TParameter, TItem> : ParameterizedSourceBase<TParameter, ImmutableArray<TItem>>, IParameterizedItemSource<TParameter, TItem>
	{
		public override ImmutableArray<TItem> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		public abstract IEnumerable<TItem> Yield( TParameter parameter );
	}
}
