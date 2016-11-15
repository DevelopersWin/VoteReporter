using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Parameterized
{
	public class ParameterizedItemCache<TParameter, TItem> : DecoratedCache<TParameter, ImmutableArray<TItem>>, IParameterizedItemSource<TParameter, TItem>
	{
		public ParameterizedItemCache( IParameterizedItemSource<TParameter, TItem> source ) : this( source.ToDelegate() ) {}

		public ParameterizedItemCache( Func<TParameter, ImmutableArray<TItem>> factory ) : base( factory ) {}

		public IEnumerable<TItem> Yield( TParameter parameter ) => Get( parameter ).AsEnumerable();
	}
}