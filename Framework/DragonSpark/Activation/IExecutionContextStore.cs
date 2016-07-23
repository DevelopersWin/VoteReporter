using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	[Priority( Priority.Low )]
	class ExecutionContextStore : StoreBase<IExecutionContextStore>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();
		ExecutionContextStore() {}

		protected override IExecutionContextStore Get() => this;
	}

	public class ExecutionContextLocator : DeferredStore<IExecutionContextStore>
	{
		public static ExecutionContextLocator Instance { get; } = new ExecutionContextLocator();
		ExecutionContextLocator() : base( () => ExecutionContextRepository.Instance.List().First() ) {}
	}
	
	[ReaderWriterSynchronized]
	public sealed class ExecutionContextRepository : RepositoryBase<IExecutionContextStore>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( new ExecutionContextStoreCollection( EnumerableEx.Return( ExecutionContextStore.Instance ) ) ) {}

		[Writer]
		protected override void OnAdd( IExecutionContextStore entry ) => Source.Ensure( entry );

		class ExecutionContextStoreCollection : PrioritizedCollection<IExecutionContextStore>
		{
			public ExecutionContextStoreCollection( IEnumerable<IExecutionContextStore> items ) : base( items ) {}
		}
	}

	public class DefaultExecutionContextFactoryStore<T> : StoreBase<T>
	{
		readonly Func<object, T> factory;
		public DefaultExecutionContextFactoryStore( Func<object, T> factory )
		{
			this.factory = factory;
		}

		protected override T Get() => factory( Defaults.ExecutionContext() );
	}

	public interface IExecutionContextStore : IStore {}

	public class ExecutionContextStore<T> : ExecutionContextStoreBase<T> where T : class
	{
		public ExecutionContextStore( Func<T> defaultFactory = null ) : base( new Cache<T>(), defaultFactory ) {}
	}

	public class ExecutionContextStructureStore<T> : ExecutionContextStoreBase<T>
	{
		public ExecutionContextStructureStore( Func<T> defaultFactory = null ) : base( new StoreCache<T>(), defaultFactory ) {}
	}

	public abstract class ExecutionContextStoreBase<T> : WritableStore<T>
	{
		readonly Func<object> contextSource;
		readonly ICache<object, T> cache;
		readonly Func<T> defaultFactory;

		protected ExecutionContextStoreBase( ICache<T> cache, Func<T> defaultFactory = null ) : this( Defaults.ExecutionContext, cache, defaultFactory ) {}

		protected ExecutionContextStoreBase( Func<object> contextSource, ICache<object, T> cache, Func<T> defaultFactory = null )
		{
			this.contextSource = contextSource;
			this.cache = cache;
			this.defaultFactory = defaultFactory;
		}

		protected override T Get()
		{
			var context = contextSource();
			if ( defaultFactory != null && !cache.Contains( context ) )
			{
				var result = defaultFactory();
				if ( result.IsAssigned() )
				{
					cache.Set( context, result );
				}
				return result;
			}
			return cache.Get( context );
		}

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => cache.Set( contextSource(), item );
	}
}