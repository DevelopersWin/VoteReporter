using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Configuration
{
	/*class AssignConfigurationCommand<T> : AssignConfigurationCommand<object, T>
	{
		public AssignConfigurationCommand( IWritableStore<Func<object, T>> store ) : base( store ) {}
	}
	
	class AssignConfigurationCommand<TKey, TValue> : AssignValueCommand<Func<TKey, TValue>>
	{
		public AssignConfigurationCommand( IWritableStore<Func<TKey, TValue>> store ) : base( store ) {}
	}*/


	public class ParameterizedConfiguration<T> : ParameterizedConfiguration<object, T>, IWritableParameterizedConfiguration<T> where T : class
	{
		public ParameterizedConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class ParameterizedConfiguration<TKey, TValue> : ParameterizedConfigurationBase<TKey, TValue> where TKey : class where TValue : class
	{
		public ParameterizedConfiguration( Func<TKey, TValue> reference ) : base( new CacheStore<TKey,TValue>( reference ) ) {}
	}

	public class StructuredParameterizedConfiguration<T> : StructuredParameterizedConfiguration<object, T>, IWritableParameterizedConfiguration<T>
	{
		public StructuredParameterizedConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class StructuredParameterizedConfiguration<TKey, TValue> : ParameterizedConfigurationBase<TKey, TValue> where TKey : class
	{
		public StructuredParameterizedConfiguration( Func<TKey, TValue> reference ) : base( new CacheStore<StoreCache<TKey, TValue>, TKey, TValue>( reference ) ) {}
	}

	public abstract class ParameterizedConfigurationBase<TKey, TValue> : IWritableParameterizedConfiguration<TKey, TValue>, IStoreAware<Func<TKey, TValue>>
	{
		readonly IWritableStore<Func<TKey, TValue>> store;

		// protected ParameterizedConfigurationBase( Func<TKey, TValue> factory ) : this( new ExecutionContextStore<Func<TKey, TValue>>( factory ) ) {}

		protected ParameterizedConfigurationBase( IWritableStore<Func<TKey, TValue>> store )
		{
			this.store = store;
		}

		public TValue Get( TKey key ) => store.Value( key );
		public void Assign( Func<TKey, TValue> factory ) => store.Assign( factory );

		Func<TKey, TValue> IStoreAware<Func<TKey, TValue>>.Value => store.Value;
	}

	class CacheStore<TKey, TValue> : CacheStore<Cache<TKey, TValue>, TKey, TValue> where TKey : class where TValue : class
	{
		public CacheStore( Func<TKey, TValue> factory ) : base( factory ) {}
	}

	class CacheStore<TCache, TKey, TValue> : ExecutionContextStore<Func<TKey, TValue>> where TCache : ICache<TKey, TValue>
	{
		readonly static Func<Func<TKey, TValue>, TCache> Constructor = ParameterConstructor<Func<TKey, TValue>, TCache>.Default;

		public CacheStore( Func<TKey, TValue> factory ) : base( factory ) {}

		protected override void OnAssign( Func<TKey, TValue> value ) => base.OnAssign( Constructor( value ).Get );
	}
}