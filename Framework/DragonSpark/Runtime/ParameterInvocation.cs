using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	/*public interface IParameterWorkflowState
	{
		void Activate( object parameter, bool on );

		void Validate( object parameter, bool on );

		bool IsActive( object parameter );

		bool IsValidated( object parameter );
	}*/

	/*public class Assignment : Assignment<object, bool>
	{
		public Assignment( Action<object, bool> assign, object parameter )
			: base( assign, Assignments.From( parameter ), new Value<bool>( true ) ) {}
	}*/

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

	public class StoreAssign<T> : IAssign<T>
	{
		readonly IWritableStore<T> store;
		public StoreAssign( IWritableStore<T> store )
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

	/*struct DelegatedAssign<T1, T2> : IAssign<T1, T2>
	{
		readonly Action<T1, T2> assign;
		public DelegatedAssign( Action<T1, T2> assign )
		{
			this.assign = assign;
		}

		public void Assign( T1 first, T2 second ) => assign( first, second );
	}*/

	public class EnabledStateAssign : IAssign<object, bool>
	{
		readonly EnabledState value;

		public EnabledStateAssign( EnabledState value )
		{
			this.value = value;
		}

		public void Assign( object first, bool second ) => value.Enable( first, second );
	}

	// [Disposable]
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

		// protected override void OnDispose() => assign.Assign( first.Finish, second.Finish );
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

	/*public class Assignment<T> : Disposable
	{
		readonly Action<T> assign;
		readonly Value<T> first;

		public Assignment( Action<T> assign, T first ) : this( assign, new Value<T>( first ) ) {}

		public Assignment( Action<T> assign, Value<T> first )
		{
			this.assign = assign;
			this.first = first;

			assign( first.Start );
		}

		protected override void OnDispose() => assign( first.Finish );
	}*/

	/*public struct BitwiseValue : IEquatable<BitwiseValue>
	{
		readonly int value;

		public BitwiseValue( int value )
		{
			this.value = value;
		}

		public int Value => value;

		public BitwiseValue If( bool condition, int bit ) => condition ? Add( bit ) : Remove( bit );

		public BitwiseValue Add( int bit ) => new BitwiseValue( value | bit );

		public BitwiseValue Remove( int bit ) => new BitwiseValue( value & ~bit );

		public bool Contains( int bit ) => ( value & bit ) == bit;

		public bool Equals( BitwiseValue other ) => value == other.value;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is BitwiseValue && Equals( (BitwiseValue)obj ) );

		public override int GetHashCode() => Value;

		public static bool operator ==( BitwiseValue left, BitwiseValue right ) => left.Equals( right );

		public static bool operator !=( BitwiseValue left, BitwiseValue right ) => !left.Equals( right );

	/*	public static implicit operator BitwiseValue( int item ) => new BitwiseValue( item );

		public static implicit operator int( BitwiseValue item ) => item.value;#1#
	}*/

	/*public class ParameterState /*: IParameterWorkflowState#1#
	{
		public ParameterState() : this( new EnabledState(), new EnabledState() ) {}

		public ParameterState( EnabledState active, EnabledState valid )
		{
			Active = active;
			Valid = valid;
		}

		/*public ParameterWorkflowState()
		{
			Active = new BitwiseValue();
		}#1#

		public EnabledState Active { get; }

		public EnabledState Valid { get; }

		/*readonly static object Null = new object();

		int active, valid;

		public void Activate( object parameter, bool on ) => Set( ref active, parameter, @on );

		public bool IsActive( object parameter ) => Get( active, parameter );

		public void Validate( object parameter, bool on ) => Set( ref valid, parameter, @on );

		public bool IsValidated( object parameter ) => Get( valid, parameter );

		static void Set( ref int store, object parameter, bool on )
		{
			var @checked = ( parameter ?? Null ).GetHashCode();
			if ( on )
			{
				store |= @checked;
			}
			else
			{
				store &= ~@checked;
			}
		}

		static bool Get( int store, object parameter )
		{
			var code = ( parameter ?? Null ).GetHashCode();
			return ( store & code ) == code;
		}#1#
	}*/

	public interface IParameterValidator
	{
		bool IsValid( object parameter );

		// object Execute( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IGenericParameterValidator
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( (TParameter)parameter );

		public bool Handles( object parameter ) => parameter is TParameter;

		public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
	}

	public class FactoryAdapter : IParameterValidator
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public bool IsValid( object parameter ) => factory.CanCreate( parameter );

		// public object Execute( object parameter ) => factory.Create( parameter );
	}

	public class CommandAdapter : IParameterValidator
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( parameter );
	}

	public class CommandAdapter<T> : IGenericParameterValidator
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( (T)parameter );

		public bool Handles( object parameter ) => parameter is T;

		public object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}
	}
}
