﻿using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class DecoratedSourceCache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class
	{
		readonly ISourceCache<TInstance, TValue> inner;

		public DecoratedSourceCache() : this( instance => default(TValue) ) {}
		public DecoratedSourceCache( Func<TInstance, TValue> create ) : this( new WritableSourceCache<TInstance, TValue>( create ) ) {}

		public DecoratedSourceCache( ISourceCache<TInstance, TValue> inner )
		{
			this.inner = inner;
		}

		public override bool Contains( TInstance instance ) => inner.Contains( instance );

		public override void Set( TInstance instance, [Optional]TValue value ) => inner.Get( instance ).Assign( value );

		public override TValue Get( TInstance instance ) => inner.Get( instance ).Get();

		public override bool Remove( TInstance instance ) => inner.Remove( instance );
	}
}