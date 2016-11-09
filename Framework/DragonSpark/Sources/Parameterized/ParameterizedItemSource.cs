using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class ParameterizedItemSourceBase<TParameter, TItem> : ParameterizedSourceBase<TParameter, ImmutableArray<TItem>>
	{
		public override ImmutableArray<TItem> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		public abstract IEnumerable<TItem> Yield( TParameter parameter );
	}

	public class SourcedItemParameterizedSource<TParameter, TItem> : ParameterizedItemSourceBase<TParameter, TItem>
	{
		readonly IParameterizedSource<TParameter, TItem>[] sources;

		public SourcedItemParameterizedSource( params IParameterizedSource<TParameter, TItem>[] sources )
		{
			this.sources = sources;
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
