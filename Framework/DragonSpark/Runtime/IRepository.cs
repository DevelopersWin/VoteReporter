using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Runtime
{
	public interface IRepository<T> : IComposable<T>
	{
		ImmutableArray<T> List();
	}

	public interface IComposable<in T>
	{
		void Add( T instance );
	}


	public sealed class Disposables : RepositoryBase<IDisposable>, IDisposable
	{
		readonly IDisposable disposable;

		public static IStore<Disposables> Instance { get; } = new ExecutionScope<Disposables>( () => new Disposables() );
		Disposables() : base( new PurgingCollection<IDisposable>() )
		{
			disposable = new DelegatedDisposable( OnDispose );
		}

		void OnDispose() => List().Each( entry => entry.Dispose() );
		public void Dispose() => disposable.Dispose();
	}

	public abstract class RepositoryBase<T> : IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( ICollection<T> source )
		{
			Source = source;
		}

		protected ICollection<T> Source { get; }

		public void Add( T instance ) => OnAdd( instance );

		protected virtual void OnAdd( T entry ) => Source.Add( entry );

		public virtual ImmutableArray<T> List() => Query().ToImmutableArray();

		protected virtual IEnumerable<T> Query() => Source;
	}

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