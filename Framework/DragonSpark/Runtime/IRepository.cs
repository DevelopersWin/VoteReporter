using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Runtime
{
	public interface IEntryRepository<T> : IRepository<Entry<T>>
	{
		void Insert( T entry );

		void Add( T entry );

		ImmutableArray<T> Items();
	}

	public interface IRepository<T>
	{
		void Insert( T entry );

		void Add( T entry );

		ImmutableArray<T> List();
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

	public abstract class EntryRepositoryBase<TItem> : EntryRepositoryBase<Entry<TItem>, TItem>
	{
		protected EntryRepositoryBase() {}

		protected EntryRepositoryBase( IEnumerable<TItem> items ) : base( items ) {}
		protected EntryRepositoryBase( IList<Entry<TItem>> store ) : base( store ) {}

		protected override Entry<TItem> Create( TItem item ) => new Entry<TItem>( item );
	}

	public abstract class EntryRepositoryBase<TEntry, TItem> : RepositoryBase<TEntry>, IEntryRepository<TItem> where TEntry : Entry<TItem>
	{
		readonly Action<TEntry> insert;
		readonly Action<TEntry> add;

		protected EntryRepositoryBase() : this( Items<TItem>.Default ) {}

		protected EntryRepositoryBase( IEnumerable<TItem> items ) : this( items, new List<TEntry>() ) {}

		protected EntryRepositoryBase( IList<TEntry> store ) : this( Items<TItem>.Default, store ) {}

		EntryRepositoryBase( IEnumerable<TItem> items, IList<TEntry> store ) : base( store )
		{
			items.Each( Add );
			insert = Insert;
			add = Add;
		}

		public void Add( TItem entry ) => add( Create( entry ) );
		void IRepository<Entry<TItem>>.Add( Entry<TItem> entry ) => entry.As( add );

		public void Insert( TItem entry ) => insert( Create( entry ) );
		void IRepository<Entry<TItem>>.Insert( Entry<TItem> entry ) => entry.As( insert );

		ImmutableArray<Entry<TItem>> IRepository<Entry<TItem>>.List() => base.List().CastArray<Entry<TItem>>();
		
		protected abstract TEntry Create( TItem item );

		public ImmutableArray<TItem> Items() => Query().Select( entry => entry.Value ).ToImmutableArray();
	}

	public abstract class PurgingRepositoryBase<T> : RepositoryBase<T>
	{
		protected PurgingRepositoryBase() {}
		protected PurgingRepositoryBase( IEnumerable<T> items ) : base( items ) {}
		protected PurgingRepositoryBase( IList<T> store ) : base( store ) {}

		protected override IEnumerable<T> Query() => Store.Purge().Prioritize();
	}

	public abstract class RepositoryBase<T> : IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( [Required] IList<T> store )
		{
			Store = store;
		}

		public IList<T> Store { get; }

		public void Insert( T entry ) => OnInsert( entry );
		protected virtual void OnInsert( T entry ) => Store.Insert( 0, entry );

		public void Add( T entry ) => OnAdd( entry );

		protected virtual void OnAdd( T entry ) => Store.Add( entry );

		public virtual ImmutableArray<T> List() => Query().ToImmutableArray();

		protected virtual IEnumerable<T> Query() => Store.Prioritize();
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