using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace DragonSpark.Runtime
{
	public interface IRepository<T>
	{
		void Add( T entry );

		IEnumerable<T> List();
	}

	public interface IDisposableRepository : IRepository<IDisposable>, IDisposable {}

	public class DisposableRepository : RepositoryBase<IDisposable>, IDisposableRepository
	{
		public void Dispose() => Store.Purge().Each( entry => entry.Item.Dispose() );
	}

	public abstract class RepositoryBase<T> : RepositoryBase<Entry<T>, T>, IRepository<T>
	{
		protected RepositoryBase() {}

		protected RepositoryBase( ICollection<Entry<T>> store ) : base( store ) {}

		public void Add( T entry ) => Add( new Entry<T>( entry ) );

		IEnumerable<T> IRepository<T>.List() => Query().Select( entry => entry.Item ).ToImmutableList();
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
	}

	public class Entry<T> : FixedValue<T>, IAllowsPriority
	{
		public Entry( [Required] T item, Priority priority = Priority.Normal )
		{
			Assign( item );
			Priority = priority;
		}

		public Priority Priority { get; }
	}
}