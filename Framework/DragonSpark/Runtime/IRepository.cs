using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Runtime.Stores;

namespace DragonSpark.Runtime
{
	public interface IEntryRepository<T> : IRepository<Entry<T>>
	{
		void Add( T entry );

		IEnumerable<T> Items();
	}

	public interface IRepository<T>
	{
		void Add( T entry );

		IEnumerable<T> List();
	}

	public interface IDisposableRepository : IRepository<IDisposable>, IDisposable {}

	public class DisposableRepository : RepositoryBase<IDisposable>, IDisposableRepository
	{
		public void Dispose()
		{
			var entries = Store.Purge();
			entries.Each( entry => entry.Dispose() );
		}
	}

	public abstract class EntryRepositoryBase<TItem> : EntryRepositoryBase<Entry<TItem>, TItem>
	{
		protected EntryRepositoryBase() {}
		protected EntryRepositoryBase( IEnumerable<TItem> items ) : base( items ) {}
		protected EntryRepositoryBase( ICollection<Entry<TItem>> store ) : base( store ) {}

		protected override Entry<TItem> Create( TItem item ) => new Entry<TItem>( item );
	}

	public abstract class EntryRepositoryBase<TEntry, TItem> : RepositoryBase<TEntry>, IEntryRepository<TItem> where TEntry : Entry<TItem>
	{
		protected EntryRepositoryBase() {}

		protected EntryRepositoryBase( IEnumerable<TItem> items )
		{
			items.Each( Add );
		}

		protected EntryRepositoryBase( ICollection<TEntry> store ) : base( store ) {}

		void IRepository<Entry<TItem>>.Add( Entry<TItem> entry ) => entry.As<TEntry>( Add );

		IEnumerable<Entry<TItem>> IRepository<Entry<TItem>>.List() => base.List();

		public void Add( TItem entry ) => Add( Create( entry ) );

		protected abstract TEntry Create( TItem item );

		public IEnumerable<TItem> Items()
		{
			var enumerable = Query();
			var result = enumerable.Select( entry => entry.Value ).ToImmutableList();
			return result;
		}
	}

	public abstract class RepositoryBase<T> : IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( [Required] ICollection<T> store )
		{
			Store = store;
		}

		protected ICollection<T> Store { get; }

		public void Add( T entry ) => OnAdd( entry );

		protected virtual void OnAdd( T entry ) => Store.Add( entry );

		public virtual IEnumerable<T> List() => Query().ToImmutableArray();

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