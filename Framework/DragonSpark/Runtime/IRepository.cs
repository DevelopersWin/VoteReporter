using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Runtime
{
	/*public interface IStoreRepository<T> : IRepository<IStore<T>>
	{
		void Add( T instance );

		ImmutableArray<T> Instances();
	}*/

	public interface IRepository<T> : IComposable<T>
	{
		ImmutableArray<T> List();
	}

	public interface IComposable<in T>
	{
		void Add( T instance );
	}

	/*public interface IDisposableRepository : IRepository<IDisposable>, IDisposable {}

	public class DisposableRepository : RepositoryBase<IDisposable>, IDisposableRepository
	{
		public void Dispose()
		{
			var entries = Store.Purge();
			entries.Each( entry => entry.Dispose() );
		}
	}*/

	/*public abstract class StoreRepositoryBase<TItem> : StoreRepositoryBase<Entry<TItem>, TItem>
	{
		protected StoreRepositoryBase() {}

		protected StoreRepositoryBase( IEnumerable<TItem> items ) : base( items ) {}
		protected StoreRepositoryBase( IList<Entry<TItem>> store ) : base( store ) {}

		protected override Entry<TItem> Create( TItem item ) => new Entry<TItem>( item );
	}*/

	/*public abstract class StoreRepositoryBase<T> : RepositoryBase<IStore<T>>, IStoreRepository<T>
	{
		// readonly Action<IStore<T>> insert;
		readonly Action<IStore<T>> add;

		protected StoreRepositoryBase() : this( Items<T>.Default ) {}

		protected StoreRepositoryBase( IEnumerable<T> items ) : this( items, new List<IStore<T>>() ) {}

		protected StoreRepositoryBase( IList<IStore<T>> store ) : this( Items<T>.Default, store ) {}

		StoreRepositoryBase( IEnumerable<T> items, IList<IStore<T>> store ) : base( store )
		{
			items.Each( Add );
			// insert = Insert;
			add = Add;
		}

		public void Add( T entry ) => add( Create( entry ) );
		// void IRepository<Entry<TItem>>.Add( Entry<TItem> entry ) => entry.As( add );

		// public void Insert( T instance ) => insert( Create( instance ) );
		// void IRepository<Entry<TItem>>.Insert( Entry<TItem> entry ) => entry.As( insert );

		// ImmutableArray<Entry<TItem>> IRepository<Entry<TItem>>.List() => base.List().CastArray<Entry<TItem>>();
		
		protected abstract IStore<T> Create( T instance );

		public ImmutableArray<T> Instances() => Query().Select( entry => entry.Value ).ToImmutableArray();
	}*/

	/*public abstract class PurgingRepositoryBase<T> : RepositoryBase<T>
	{
		protected PurgingRepositoryBase() {}
		protected PurgingRepositoryBase( IEnumerable<T> items ) : base( items ) {}
		protected PurgingRepositoryBase( IList<T> store ) : base( store ) {}

		protected override IEnumerable<T> Query() => Store.Purge().Prioritize();
	}*/

	public abstract class RepositoryBase<T> : IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( ICollection<T> source )
		{
			Source = source;
		}

		protected ICollection<T> Source { get; }

		/*public void Insert( T entry ) => OnInsert( entry );
		protected virtual void OnInsert( T entry ) => Store.Insert( 0, entry );*/

		public void Add( T instance ) => OnAdd( instance );

		protected virtual void OnAdd( T entry ) => Source.Add( entry );

		public virtual ImmutableArray<T> List() => Query().ToImmutableArray();

		protected virtual IEnumerable<T> Query() => Source;
	}

	/*public abstract class RepositoryBase<T> : RepositoryBase<Entry<T>, T>, IRepository<T>
	{
		protected RepositoryBase() {}

		protected RepositoryBase( IEnumerable<T> items )
		{
			items.Each( Add );
		}

		protected RepositoryBase( ICollection<Entry<T>> store ) : base( store ) {}

		public void Add( T entry ) => Add( new Entry<T>( entry ) );

		IEnumerable<T> IRepository<T>.List() => ;
	}

	public abstract class RepositoryBase<TEntry, TItem> : IRepository<TEntry> where TEntry : Entry<TItem>
	{
		protected RepositoryBase() : this( new List<TEntry>() ) {}

		protected RepositoryBase( [Required] ICollection<TEntry> store )
		{
			Store = store;
		}

		protected ICollection<TEntry> Store { get; }

		public void Add( TEntry entry ) => OnAdd( entry );

		protected virtual void OnAdd( TEntry entry ) => Store.Add( entry );

		public virtual IEnumerable<TEntry> List() => Query().ToImmutableArray();

		protected virtual IEnumerable<TEntry> Query() => Store.Prioritize();
	}*/

	public class Entry<T> : FixedStore<T>, IPriorityAware
	{
		public Entry( [Required] T item, Priority priority = Priority.Normal )
		{
			Assign( item );
			Priority = priority;
		}

		public Priority Priority { get; }
	}
}