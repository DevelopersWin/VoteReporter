using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedItemSource<in TParameter, TItem> : IParameterizedSource<TParameter, ImmutableArray<TItem>>
	{
		IEnumerable<TItem> Yield( TParameter parameter );
	}
}