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
		readonly static Func<object> Store = new DelegatedCachedSource<IExecutionContextStore>( () => ExecutionContextRepository.Instance.List().First() ).Delegate();

		public static object Current() => Store();
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

	public class ExecutionScopedStore<T> : StoreBase<T>
	{
		readonly Func<object, T> factory;
		public ExecutionScopedStore( Func<object, T> factory )
		{
			this.factory = factory;
		}

		protected override T Get() => factory( Execution.Current() );
	}

	/*public class ActivatedParameterizedSource<T> : DecoratedCache<T>, IAssignableParameterizedSource<T>
	{
		readonly Func<object, T> factory;

		public ActivatedParameterizedSource( Func<T> factory ) : this( factory.Wrap() ) {}

		public ActivatedParameterizedSource( Func<object, T> factory ) : base( factory )
		{
			this.factory = factory;
		}

		public override T Get( object parameter ) => Contains( parameter ) ? Get( parameter ) : factory( parameter );
	}*/

	public interface IExecutionContextStore : ISource {}

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