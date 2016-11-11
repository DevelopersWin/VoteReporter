using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public class CompositeFactory<TParameter, TItem> : ParameterizedItemSourceBase<TParameter, TItem>
	{
		readonly ImmutableArray<Func<TParameter, TItem>> sources;

		public CompositeFactory( params IParameterizedSource<TParameter, TItem>[] sources ) : this ( sources.Select( source => source.ToDelegate() ).Fixed() ) {}

		public CompositeFactory( params Func<TParameter, TItem>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<TItem> Yield( TParameter parameter )
		{
			foreach ( var source in sources )
			{
				var instance = source( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}