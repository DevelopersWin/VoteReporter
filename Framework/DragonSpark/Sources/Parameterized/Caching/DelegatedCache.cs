using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class DelegatedCache<TKey, TValue> : CacheBase<TKey, TValue>
	{
		readonly Func<TKey, bool> contains;
		readonly Func<TKey, TValue> get;
		readonly Action<TKey, TValue> set;
		readonly Func<TKey, bool> remove;

		public DelegatedCache( Func<TKey, bool> contains, Func<TKey, TValue> get, Action<TKey, TValue> set, Func<TKey, bool> remove  )
		{
			this.contains = contains;
			this.get = get;
			this.set = set;
			this.remove = remove;
		}

		public override TValue Get( TKey parameter ) => get( parameter );

		public override void Set( TKey instance, TValue value ) => set( instance, value );
		public override bool Contains( TKey instance ) => contains( instance );

		public override bool Remove( TKey instance ) => remove( instance );
	}
}