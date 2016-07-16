using DragonSpark.Configuration;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Activation
{
	class ExecutionContext : StoreBase<IExecutionContext>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		protected override IExecutionContext Get() => this;
	}

	public class ExecutionContextLocator : WritableConfigurationBase<IExecutionContext>
	{
		public static ExecutionContextLocator Instance { get; } = new ExecutionContextLocator();
		ExecutionContextLocator() : base( () => ExecutionContext.Instance ) {}
	}

	/*[ReaderWriterSynchronized]
	public sealed class ExecutionContextRepository : RepositoryBase<IExecutionContext>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( ExecutionContext.Instance.ToItem() ) {}

		[Writer]
		protected override void OnAdd( IExecutionContext entry ) => Store.Ensure( entry );

		[Writer]
		protected override void OnInsert( IExecutionContext entry )
		{
			if ( !Store.Contains( entry ) )
			{
				base.OnInsert( entry );
			}
		}

		[Reader]
		public object Current() => Get<object>();

		[Reader]
		public T Get<T>() => Query().Select( context => context.Value ).OfType<T>().FirstAssigned();

		[Reader]
		protected override IEnumerable<IExecutionContext> Query() => Store;
	}*/

	/*public class MethodContext : ExecutionContextCachedBase<MethodBase>
	{
		public static MethodContext Instance { get; } = new MethodContext();
		MethodContext() {}
	}*/

	public interface IExecutionContext : IStore {}

	public abstract class ExecutionContextCachedBase<T> : ExecutionContextStoreBase<T> where T : class
	{
		protected ExecutionContextCachedBase() : base( new Cache<T>() ) {}
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

		public override void Assign( T item ) => cache.Set( contextSource(), item );
	}

	/*public class AssignExecutionContextCommand : DelegatedCommand<IExecutionContext>
	{
		public static AssignExecutionContextCommand Instance { get; } = new AssignExecutionContextCommand();
		AssignExecutionContextCommand() : base( ExecutionContextRepository.Instance.Insert ) {}
	}*/
}