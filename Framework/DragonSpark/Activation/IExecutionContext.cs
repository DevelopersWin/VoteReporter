using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Activation
{
	/*public interface IExecutionContext : IStore {}

	public static class Execution
	{
		static Execution()
		{
			Initialize( ExecutionContext.Instance );
		}

		public static void Initialize( IExecutionContext current )
		{
			Context = current;
		}	static IExecutionContext Context { get; set; }

		// public static void Assign( object current ) => Context.Assign( current );

		public static object GetCurrent() => Context.Value;
	}*/

	/*public class ExecutionContext : StoreBase<object>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		protected override object Get() => this;
	}*/

	class ExecutionContextRepository : RepositoryBase<IExecutionContext>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( /*DefaultExecutionContext.Instance.ToItem()*/ ) {}

		protected override void OnAdd( IExecutionContext entry ) => Store.Ensure( entry );

		protected override void OnInsert( IExecutionContext entry )
		{
			if ( !Store.Contains( entry ) )
			{
				base.OnInsert( entry );
			}
		}

		public object Current() => Query().FirstOrDefault()?.Value;

		public T Get<T>() => Query().Select( context => context.Value ).FirstOrDefaultOfType<T>();

		protected override IEnumerable<IExecutionContext> Query() => Store;
	}

	public class ApplicationContext : CompositeCommand
	{
		public ApplicationContext( params ICommand[] commands ) : base( commands ) {}
	}

	

	class MethodContext : ExecutionContextCachedBase<MethodBase>
	{
		public static MethodContext Instance { get; } = new MethodContext();
		MethodContext() {}
	}

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

	[Synchronized]
	public class AssignExecutionContextCommand : DelegatedCommand<IExecutionContext>
	{
		public static AssignExecutionContextCommand Instance { get; } = new AssignExecutionContextCommand();
		AssignExecutionContextCommand() : base( ExecutionContextRepository.Instance.Insert ) {}
	}
}