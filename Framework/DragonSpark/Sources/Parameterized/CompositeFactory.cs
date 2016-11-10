using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public class CompositeFactory<TParameter, TItem> : ParameterizedItemSourceBase<TParameter, TItem>
	{
		readonly ImmutableArray<IParameterizedSource<TParameter, TItem>> sources;

		public CompositeFactory( params IParameterizedSource<TParameter, TItem>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<TItem> Yield( TParameter parameter )
		{
			foreach ( var source in sources )
			{
				var instance = source.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}