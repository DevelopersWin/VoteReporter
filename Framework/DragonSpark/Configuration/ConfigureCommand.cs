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

	
	public class WritableConfiguration<T> : WritableConfiguration<object, T>, IWritableConfiguration<T> where T : class
	{
		public WritableConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class WritableConfiguration<TKey, TValue> : WritableConfigurationBase<TKey, TValue> where TKey : class where TValue : class
	{
		public WritableConfiguration( Func<TKey, TValue> reference ) : base( new ConfigurableCache<TKey,TValue>( reference ) ) {}
	}

	public class WritableStructureConfiguration<T> : WritableStructureConfiguration<object, T>, IWritableConfiguration<T>
	{
		public WritableStructureConfiguration( Func<object, T> reference ) : base( reference ) {}
	}

	public class WritableStructureConfiguration<TKey, TValue> : WritableConfigurationBase<TKey, TValue> where TKey : class
	{
		public WritableStructureConfiguration( Func<TKey, TValue> reference ) : base( new ConfigurableCache<StoreCache<TKey, TValue>, TKey, TValue>( reference ) ) {}
	}

	public abstract class WritableConfigurationBase<TKey, TValue> : ExecutionContextStoreBase<Func<TKey, TValue>>, IWritableConfiguration<TKey, TValue>
	{
		protected WritableConfigurationBase( ICache<Func<TKey, TValue>> cache ) : base( cache ) {}

		public TValue Get( TKey key ) => Value( key );
	}

	public interface IWritableConfiguration<T> : IWritableConfiguration<object, T> {}

	public interface IWritableConfiguration<TKey, TValue> : IConfiguration<TKey, TValue>
	{
		void Assign( Func<TKey, TValue> factory );
	}

	class ConfigurableCache<TKey, TValue> : ConfigurableCache<Cache<TKey, TValue>, TKey, TValue> where TKey : class where TValue : class
	{
		public ConfigurableCache( Func<TKey, TValue> factory ) : base( factory ) {}
	}

	class ConfigurableCache<TCache, TKey, TValue> : Cache<Func<TKey, TValue>> where TCache : ICache<TKey, TValue>
	{
		readonly static Func<Func<TKey, TValue>, TCache> Constructor = ParameterConstructor<Func<TKey, TValue>, TCache>.Default;

		public ConfigurableCache( Func<TKey, TValue> factory ) : base( new Func<TKey, TValue>( Constructor( factory ).Get ).Wrap().ToDelegate() ) {}

		public override void Set( object instance, [Required]Func<TKey, TValue> value ) => base.Set( instance, Constructor( value ).Get );
	}
}