using DragonSpark.Configuration;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Activation
{
	public static class Execution
	{
		readonly static FixedStore<ISource> Store = new FixedStore<ISource>( ExecutionContext.Instance );
		readonly static Func<object> Source = Store.Delegate();
		public static IAssignable<ISource> Context { get; } = Store;

		public static object Current() => Source();
	}

	[Priority( Priority.Low )]
	class ExecutionContext : StoreBase<object>
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		ExecutionContext() {}

		protected override object Get() => this;
	}

	/*[ReaderWriterSynchronized]
	public sealed class ExecutionContextRepository : RepositoryBase<IExecutionContextSource>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( new PrioritizedCollection<IExecutionContextSource>( EnumerableEx.Return( ExecutionContext.Instance ) ) ) {}

		[Writer]
		protected override void OnAdd( IExecutionContextSource entry ) => Source.Ensure( entry );
	}*/

	public class ScopedStore<T> : StoreBase<T>
	{
		readonly Func<object, T> factory;
		public ScopedStore( Func<object, T> factory )
		{
			this.factory = factory;
		}

		protected override T Get() => factory( Execution.Current() );
	}

	public class ExecutionScope<T> : WritableStore<T>, IParameterizedSource
	{
		readonly IAssignableParameterizedSource<T> source;

		public ExecutionScope() : this( () => default(T) ) {}

		public ExecutionScope( Func<T> defaultFactory ) : this( CacheFactory.Create( defaultFactory ) ) {}

		public ExecutionScope( IAssignableParameterizedSource<T> source )
		{
			this.source = source;
		}

		protected override T Get() => source.Get( Execution.Current() );

		public override void Assign( T item ) => source.Set( Execution.Current(), item );
		public object Get( object parameter ) => source.Get( parameter  );
	}

	public class Configuration<T> : ExecutionScope<T>, IConfiguration<T>, IAssignable<Func<object, T>>
	{
		readonly IConfigurableCache<T> source;

		public Configuration() : this( () => default(T) ) {}
		public Configuration( Func<T> defaultFactory ) : this( new Cache( defaultFactory ) ) {}

		public Configuration( IConfigurableCache<T> source ) : base( source )
		{
			this.source = source;
		}

		public void Assign( Func<object, T> item ) => source.Assign( item );

		public void Assign( Func<T> item ) => Assign( item.Wrap() );

		sealed class Cache : ConfigurableScopedCache<T>
		{
			public Cache( Func<T> defaultFactory ) : base( defaultFactory ) {}

			
		}
	}
}