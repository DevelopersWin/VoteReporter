using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Threading;
using System;
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
		ExecutionContextRepository() : base( EnumerableEx.Return( ExecutionContextStore.Instance ) ) {}

		[Writer]
		protected override void OnAdd( IExecutionContextStore entry ) => Store.Ensure( entry );
	}

	public interface IExecutionContextStore : IStore {}

	public class ExecutionContextStore<T> : ExecutionContextReferenceStoreBase<T> where T : class
	{
		public ExecutionContextStore() : base( new Cache<T>() ) {}

		public ExecutionContextStore( T reference ) : base( reference, new Cache<T>() ) {}
	}

	public class ExecutionContextStructureStore<T> : ExecutionContextReferenceStoreBase<T>
	{
		public ExecutionContextStructureStore() : base( new StoreCache<T>() ) {}
		public ExecutionContextStructureStore( T reference ) : base( reference, new StoreCache<T>() ) {}
	}

	public abstract class ExecutionContextReferenceStoreBase<T> : ExecutionContextStoreBase<T>
	{
		readonly IWritableStore<T> reference = new FixedStore<T>();

		protected ExecutionContextReferenceStoreBase( ICache<T> cache ) : base( cache ) {}

		protected ExecutionContextReferenceStoreBase( T reference, ICache<T> cache ) : base( cache )
		{
			Assign( reference );
		}

		protected override T Get()
		{
			if ( !Contains() )
			{
				Assign( reference.Value );
			}

			return base.Get();
		}

		protected override void OnAssign( T item )
		{
			reference.Assign( item );
			base.OnAssign( item );
		}
	}

	public abstract class ExecutionContextStoreBase<T> : WritableStore<T>
	{
		readonly Func<object> contextSource;
		readonly ICache<object, T> cache;

		protected ExecutionContextStoreBase( ICache<T> cache ) : this( Defaults.ExecutionContext, cache ) {}

		protected ExecutionContextStoreBase( Func<object> contextSource, ICache<object, T> cache )
		{
			this.contextSource = contextSource;
			this.cache = cache;
		}

		protected bool Contains() => cache.Contains( contextSource() );

		protected override T Get() => cache.Get( contextSource() );

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => cache.Set( contextSource(), item );
	}
}