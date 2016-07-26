using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Threading;
using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public static class Execution
	{
		readonly static ISource<IExecutionContextStore> Store = new DeferredStore<IExecutionContextStore>( () => ExecutionContextRepository.Instance.List().First() );

		public static object Current() => Store.Get().Value;
	}

	[Priority( Priority.Low )]
	class ExecutionContextStore : StoreBase<IExecutionContextStore>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();
		ExecutionContextStore() {}

		protected override IExecutionContextStore Get() => this;
	}

	[ReaderWriterSynchronized]
	public sealed class ExecutionContextRepository : RepositoryBase<IExecutionContextStore>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( new PrioritizedCollection<IExecutionContextStore>( EnumerableEx.Return( ExecutionContextStore.Instance ) ) ) {}

		[Writer]
		protected override void OnAdd( IExecutionContextStore entry ) => Source.Ensure( entry );
	}

	public class DefaultExecutionContextFactoryStore<T> : StoreBase<T>
	{
		readonly Func<object, T> factory;
		public DefaultExecutionContextFactoryStore( Func<object, T> factory )
		{
			this.factory = factory;
		}

		protected override T Get() => factory( Execution.Current() );
	}

	public interface IExecutionContextStore : IStore {}

	public class ExecutionScope<T> : WritableStore<T>
	{
		readonly ICache<T> cache;

		public ExecutionScope() : this( () => default(T) ) {}

		public ExecutionScope( Func<T> defaultFactory ) : this( CacheFactory.Create( defaultFactory ) ) {}

		public ExecutionScope( ICache<T> cache )
		{
			this.cache = cache;
		}

		protected override T Get() => cache.Get( Execution.Current() );

		public override void Assign( T item ) => cache.Set( Execution.Current(), item );
	}

	public class Configuration<T> : ExecutionScope<T>, IConfiguration<T>, IAssignable<Func<object, T>>
	{
		readonly IConfigurableCache<T> cache;

		public Configuration() : this( () => default(T) ) {}
		public Configuration( Func<T> defaultFactory ) : this( new ConfigurableCache<T>( defaultFactory.Wrap().Create ) ) {}

		public Configuration( IConfigurableCache<T> cache ) : base( cache )
		{
			this.cache = cache;
		}

		public void Assign( Func<object, T> item ) => cache.Assign( item );

		public void Assign( Func<T> item ) => Assign( item.Wrap().ToDelegate() );
	}
}