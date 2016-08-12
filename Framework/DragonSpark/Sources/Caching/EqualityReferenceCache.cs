using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Caching
{
	public class EqualityReferenceCache<TInstance, TValue> : DecoratedCache<TInstance, TValue> where TInstance : class
	{
		readonly static Func<TInstance, TInstance> DefaultSource = EqualityReference<TInstance>.Instance.Get;

		readonly Func<TInstance, TInstance> equalitySource;

		public EqualityReferenceCache() : this( instance => default(TValue) ) {}
		public EqualityReferenceCache( Func<TInstance, TValue> create ) : this( create, DefaultSource ) {}
		public EqualityReferenceCache( Func<TInstance, TValue> create , Func<TInstance, TInstance> equalitySource ) : this( CacheFactory.Create( create ), equalitySource ) {}

		public EqualityReferenceCache( ICache<TInstance, TValue> inner, Func<TInstance, TInstance> equalitySource ) : base( inner )
		{
			this.equalitySource = equalitySource;
		}

		public override bool Contains( TInstance instance ) => base.Contains( equalitySource( instance ) );

		public override bool Remove( TInstance instance ) => base.Remove( equalitySource( instance ) );

		public override void Set( TInstance instance, [Optional]TValue value ) => base.Set( equalitySource( instance ), value );

		public override TValue Get( TInstance instance ) => base.Get( equalitySource( instance )  );
	}
}