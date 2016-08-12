using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Sources;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public class DelegatedDisposable : Disposable
	{
		readonly Action action;
		public DelegatedDisposable( Action action )
		{
			this.action = action;
		}

		protected override void OnDispose( bool disposing )
		{
			if ( disposing )
			{
				action();
			}
		}
	}

	public class Disposable : IDisposable
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		~Disposable()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing )
		{
			if ( monitor.Apply() )
			{
				OnDispose( disposing );
			}
		}

		protected virtual void OnDispose( bool disposing ) {}
	}

	public static class Assignments
	{
		public static Value<T> From<T>( T item ) => new Value<T>( item, item );
	}

	public struct Value<T>
	{
		public Value( T start, T finish = default(T) )
		{
			Start = start;
			Finish = finish;
		}

		public T Start { get; }
		public T Finish { get; }
	}

	public class CacheAssign<T1, T2> : IAssign<T1, T2>
	{
		readonly ICache<T1, T2> cache;
		public CacheAssign( ICache<T1, T2> cache )
		{
			this.cache = cache;
		}

		public void Assign( T1 first, T2 second ) => cache.Set( first, second );
	}

	public class SourceAssignment<T> : IAssign<T>
	{
		readonly IAssignable<T> store;
		public SourceAssignment( IAssignable<T> store )
		{
			this.store = store;
		}

		public void Assign( T first ) => store.Assign( first );
	}

	public interface IAssign<in T1, in T2>
	{
		void Assign( T1 first, T2 second );
	}

	public interface IAssign<in T>
	{
		void Assign( T first );
	}

	public class CollectionAssign<T> : IAssign<T, CollectionAction>
	{
		readonly IList collection;

		public CollectionAssign( IList collection )
		{
			this.collection = collection;
		}

		public void Assign( T first, CollectionAction second )
		{
			switch ( second )
			{
				case CollectionAction.Add:
					collection.Add( first );
					break;
				case CollectionAction.Remove:
					collection.Remove( first );
					break;
			}
		}
	}

	public class DictionaryAssign<T1, T2> : IAssign<T1, T2>
	{
		readonly IDictionary<T1, T2> dictionary;

		public DictionaryAssign( IDictionary<T1, T2> dictionary )
		{
			this.dictionary = dictionary;
		}

		public void Assign( T1 first, T2 second ) => dictionary[first] = second;
	}

	public enum CollectionAction { Add, Remove }

	/*public class EnabledStateAssign : IAssign<object, bool>
	{
		readonly EnabledState value;

		public EnabledStateAssign( EnabledState value )
		{
			this.value = value;
		}

		public void Assign( object first, bool second ) => value.Enable( first, second );
	}*/

	public class Assignment<T1, T2> : IDisposable
	{
		readonly IAssign<T1, T2> assign;
		readonly Value<T1> first;
		readonly Value<T2> second;

		public Assignment( IAssign<T1, T2> assign, T1 first, T2 second ) : this( assign, Assignments.From( first ), new Value<T2>( second ) ) {}

		public Assignment( IAssign<T1, T2> assign, Value<T1> first, Value<T2> second )
		{
			this.assign = assign;
			this.first = first;
			this.second = second;

			assign.Assign( first.Start, second.Start );
		}

		public void Dispose() => assign.Assign( first.Finish, second.Finish );
	}

	public class Assignment<T> : IDisposable
	{
		readonly IAssign<T> assign;
		readonly Value<T> first;
		
		public Assignment( IAssign<T> assign, Value<T> first )
		{
			this.assign = assign;
			this.first = first;
			
			assign.Assign( first.Start );
		}

		public void Dispose() => assign.Assign( first.Finish );
	}
}
