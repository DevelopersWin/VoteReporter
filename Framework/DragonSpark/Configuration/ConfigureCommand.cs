using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Configuration
{
	class AssignConfigurationCommand<T> : AssignConfigurationCommand<object, T>
	{
		public AssignConfigurationCommand( IWritableStore<Func<object, T>> store ) : base( store ) {}
	}
	
	class AssignConfigurationCommand<TKey, TValue> : AssignValueCommand<Func<TKey, TValue>>
	{
		public AssignConfigurationCommand( IWritableStore<Func<TKey, TValue>> store ) : base( store ) {}
	}

	
	public class WritableParameterizedConfiguration<T> : WritableParameterizedConfiguration<object, T>, IWritableParameterizedConfiguration<T> where T : class
	{
		public WritableParameterizedConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class WritableParameterizedConfiguration<TKey, TValue> : WritableParameterizedConfigurationBase<TKey, TValue> where TKey : class where TValue : class
	{
		public WritableParameterizedConfiguration( Func<TKey, TValue> reference ) : base( new ConfigurableCache<TKey,TValue>( reference ) ) {}
	}

	public class WritableParameterizedStructureConfiguration<T> : WritableParameterizedStructureConfiguration<object, T>, IWritableParameterizedConfiguration<T>
	{
		public WritableParameterizedStructureConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class WritableParameterizedStructureConfiguration<TKey, TValue> : WritableParameterizedConfigurationBase<TKey, TValue> where TKey : class
	{
		public WritableParameterizedStructureConfiguration( Func<TKey, TValue> reference ) : base( new ConfigurableCache<StoreCache<TKey, TValue>, TKey, TValue>( reference ) ) {}
	}

	public abstract class WritableParameterizedConfigurationBase<TKey, TValue> : ExecutionContextStoreBase<Func<TKey, TValue>>, IWritableParameterizedConfiguration<TKey, TValue>
	{
		protected WritableParameterizedConfigurationBase( ICache<Func<TKey, TValue>> cache ) : base( cache ) {}

		public TValue Get( TKey key ) => Value( key );
	}

	class ConfigurableCache<TKey, TValue> : ConfigurableCache<Cache<TKey, TValue>, TKey, TValue> where TKey : class where TValue : class
	{
		public ConfigurableCache( Func<TKey, TValue> factory ) : base( factory ) {}
	}

	class ConfigurableCache<TCache, TKey, TValue> : Cache<Func<TKey, TValue>> where TCache : ICache<TKey, TValue>
	{
		readonly static Func<Func<TKey, TValue>, TCache> Constructor = ParameterConstructor<Func<TKey, TValue>, TCache>.Default;

		public ConfigurableCache( Func<TKey, TValue> factory ) : base( new Func<TKey, TValue>( Constructor( factory ).Get ).Wrap().Create ) {}

		public override void Set( object instance, [Required]Func<TKey, TValue> value ) => base.Set( instance, Constructor( value ).Get );
	}
}