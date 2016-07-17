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

		/*[Writer]
		protected override void OnInsert( IExecutionContext entry )
		{
			if ( !Store.Contains( entry ) )
			{
				base.OnInsert( entry );
			}
		}*/

		/*[Reader]
		public object Current() => Get<object>();

		[Reader]
		public T Get<T>() => Query().Select( context => context.Value ).OfType<T>().FirstAssigned();*/

		/*[Reader]
		protected override IEnumerable<IExecutionContext> Query() => Store;*/
	}

	public interface IExecutionContextStore : IStore {}

	public class ExecutionContextStore<T> : ExecutionContextStoreBase<T> where T : class
	{
		public ExecutionContextStore() : base( new Cache<T>() ) {}

		public ExecutionContextStore( T reference ) : base( new Cache<T>() )
		{
			Assign( reference );
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

		protected override T Get() => cache.Get( contextSource() );

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => cache.Set( contextSource(), item );
	}

	/*public class AssignExecutionContextCommand : DelegatedCommand<IExecutionContext>
	{
		public static AssignExecutionContextCommand Instance { get; } = new AssignExecutionContextCommand();
		AssignExecutionContextCommand() : base( ExecutionContextRepository.Instance.Insert ) {}
	}*/
}