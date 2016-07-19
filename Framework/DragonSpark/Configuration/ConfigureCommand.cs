using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Configuration
{
	public class CachedParameterizedConfiguration<T> : CachedParameterizedConfiguration<object, T>, IParameterizedConfiguration<T> where T : class
	{
		public CachedParameterizedConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class CachedParameterizedConfiguration<TKey, TValue> : ParameterizedConfiguration<TKey, TValue> where TKey : class where TValue : class
	{
		public CachedParameterizedConfiguration( Func<TKey, TValue> reference ) : base( new CacheStore<TKey,TValue>( reference ) ) {}
	}

	public class StructuredParameterizedConfiguration<T> : StructuredParameterizedConfiguration<object, T>, IParameterizedConfiguration<T>
	{
		public StructuredParameterizedConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class StructuredParameterizedConfiguration<TKey, TValue> : ParameterizedConfiguration<TKey, TValue> where TKey : class
	{
		public StructuredParameterizedConfiguration( Func<TKey, TValue> reference ) : base( new CacheStore<StoreCache<TKey, TValue>, TKey, TValue>( reference ) ) {}
	}

	public class ParameterizedConfiguration<TKey, TValue> : IParameterizedConfiguration<TKey, TValue>, IAssignable<Func<TKey, TValue>>
	{
		readonly IWritableStore<Func<TKey, TValue>> store;

		public ParameterizedConfiguration( IWritableStore<Func<TKey, TValue>> store )
		{
			this.store = store;
		}

		public TValue Get( TKey key ) => store.Value( key );

		public void Assign( Func<TKey, TValue> factory ) => store.Assign( factory );
		void IAssignable.Assign( object item ) => store.Assign( item );
	}

	class CacheStore<TKey, TValue> : CacheStore<Cache<TKey, TValue>, TKey, TValue> where TKey : class where TValue : class
	{
		public CacheStore( Func<TKey, TValue> factory ) : base( factory ) {}
	}

	class CacheStore<TCache, TKey, TValue> : ExecutionContextStore<Func<TKey, TValue>> where TCache : ICache<TKey, TValue>
	{
		readonly static Func<Func<TKey, TValue>, TCache> Constructor = ParameterConstructor<Func<TKey, TValue>, TCache>.Default;

		public CacheStore( Func<TKey, TValue> factory ) : base( factory.Self ) {}

		protected override void OnAssign( Func<TKey, TValue> value ) => base.OnAssign( Constructor( value ).Get );
	}
}