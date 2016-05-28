using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System;
using System.Threading;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	public interface IParameterWorkflowState
	{
		void Activate( object parameter, bool on );

		void Validate( object parameter, bool on );

		bool IsActive( object parameter );

		bool IsValidated( object parameter );
	}

	public class Assignment : Assignment<object, bool>
	{
		public Assignment( Action<object, bool> assign, object parameter )
			: base( assign, From( parameter ), new Value<bool>( true ) ) {}
	}

	public class Disposable : IDisposable
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		~Disposable()
		{
			Dispose( false );
		}

		void IDisposable.Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => monitor.ApplyIf( disposing, OnDispose );

		protected virtual void OnDispose() {}
	}

	public abstract class AssignmentBase : Disposable
	{
		public static Value<T> From<T>( T item ) => new Value<T>( item, item );
	}

	public struct Value<T>
	{
		public Value( T start ) : this( start, Default<T>.Item ) {}

		public Value( T start, T finish )
		{
			Start = start;
			Finish = finish;
		}

		public T Start { get; }
		public T Finish { get; }
	}

	public class PropertyAssignment<T1, T2> : Assignment<T1, T2> where T1 : class
	{
		public PropertyAssignment( IAttachedProperty<T1, T2> assign, T1 first, T2 second ) : this( assign, From( first ), new Value<T2>( second ) ) {}

		public PropertyAssignment( IAttachedProperty<T1, T2> assign, Value<T1> first, Value<T2> second ) : base( assign.Set, first, second ) {}
	}

	public class Assignment<T1, T2> : AssignmentBase
	{
		readonly Action<T1, T2> assign;
		readonly Value<T1> first;
		readonly Value<T2> second;

		public Assignment( Action<T1, T2> assign, Value<T1> first, Value<T2> second )
		{
			this.assign = assign;
			this.first = first;
			this.second = second;

			assign( first.Start, second.Start );
		}

		protected override void OnDispose() => assign( first.Finish, second.Finish );
	}

	public class Assignment<T> : AssignmentBase
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
	}

	public struct BitwiseValueStore : IEquatable<BitwiseValueStore>
	{
		int value;

		public int Value => value;

		public int If( bool condition, int bit ) => condition ? Add( bit ) : Remove( bit );

		public int Add( int bit )
		{
			Interlocked.CompareExchange( ref value, value | bit, value );
			return value;
		}

		public int Remove( int bit )
		{
			Interlocked.CompareExchange( ref value, value & ~bit, value );
			return Value;
		}

		public bool Contains( int bit ) => ( value & bit ) == bit;

		public bool Equals( BitwiseValueStore other ) => value == other.value;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is BitwiseValueStore && Equals( (BitwiseValueStore)obj ) );

		public override int GetHashCode() => Value;

		public static bool operator ==( BitwiseValueStore left, BitwiseValueStore right ) => left.Equals( right );

		public static bool operator !=( BitwiseValueStore left, BitwiseValueStore right ) => !left.Equals( right );
	}

	public struct ParameterWorkflowState : IParameterWorkflowState
	{
		readonly static object Null = new object();

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
		}
	}

	public interface IParameterValidator
	{
		bool IsValid( object parameter );

		// object Execute( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IParameterValidator
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( (TParameter)parameter );

		// public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
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

		public object Execute( object parameter )
		{
			inner.Execute( parameter );
			return null;
		}
	}

	public class CommandAdapter<T> : IParameterValidator
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( (T)parameter );

		public object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}
	}

	public interface IInvocation<out T>
	{
		T Invoke( object parameter );
	}

	struct RelayInvocation : IInvocation<bool>
	{
		readonly RelayParameter valid;

		public RelayInvocation( RelayParameter valid )
		{
			this.valid = valid;
		}

		public bool Invoke( object parameter ) => valid.Proceed<bool>();
	}

	public struct AdapterInvocation : IInvocation<bool>
	{
		readonly IParameterValidator adapter;
		public AdapterInvocation( IParameterValidator adapter )
		{
			this.adapter = adapter;
		}

		public bool Invoke( object parameter ) => adapter.IsValid( parameter );
	}

	public struct ValidationInvocation<T> where T : IInvocation<bool>
	{
		readonly T invocation;
		readonly IParameterWorkflowState state;
		
		public ValidationInvocation( IParameterWorkflowState state, T invocation )
		{
			this.state = state;
			this.invocation = invocation;
		}

		public bool Invoke( object parameter )
		{
			var result = invocation.Invoke( parameter );
			var validated = result && !state.IsValidated( parameter ) && !state.IsActive( parameter );
			state.Validate( parameter, validated );
			return result;
		}
	}

	public struct ParameterInvocation : IInvocation<object>
	{
		readonly RelayParameter execute;
		readonly IParameterWorkflowState state;
		readonly ValidationInvocation<AdapterInvocation> validation;

		public ParameterInvocation( IParameterWorkflowState state, ValidationInvocation<AdapterInvocation> validation, RelayParameter execute )
		{
			this.state = state;
			this.validation = validation;
			this.execute = execute;
		}

		bool AsActive( object parameter )
		{
			using ( new Assignment( state.Activate, parameter ).Configured( false ) )
			{
				return validation.Invoke( parameter );
			}
		}

		object AsValid( object parameter )
		{
			using ( new Assignment( state.Validate, parameter ).Configured( false ) )
			{
				return execute.Proceed<object>();
			}
		}

		public object Invoke( object parameter ) => state.IsValidated( parameter ) || AsActive( parameter ) ? AsValid( parameter ) : null;

		/*bool Check( object parameter )
		{
			var result = state.IsValidated( parameter );
			if ( result )
			{
				state.Validate( parameter, false );
			}
			return result;
		}*/
	}
}
