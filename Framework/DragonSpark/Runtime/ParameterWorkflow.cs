using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
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

	public class Value<T>
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

		public Assignment( Action<T> assign, Value<T> first )
		{
			this.assign = assign;
			this.first = first;

			assign( first.Start );
		}

		protected override void OnDispose() => assign( first.Finish );
	}

	public class ParameterWorkflowState : IParameterWorkflowState
	{
		readonly Container active = new Container(), valid = new Container();

		public void Activate( object parameter, bool on ) => active.Set( parameter, @on );

		public bool IsActive( object parameter ) => active.Get( parameter );

		public void Validate( object parameter, bool on ) => valid.Set( parameter, @on );

		public bool IsValidated( object parameter ) => valid.Get( parameter );

		class Container // : AttachedProperty<bool>
		{
			readonly static object Null = new object();

			readonly ISet<int> store = new HashSet<int>();

			public void Set( object parameter, bool on )
			{
				var @checked = ( parameter ?? Null ).GetHashCode();
				if ( on )
				{
					store.Add( @checked );
				}
				else
				{
					store.Remove( @checked );
				}
			}

			public bool Get( object parameter ) => store.Contains( ( parameter ?? Null ).GetHashCode() );
		}
	}

	public interface IParameterAware
	{
		bool IsValid( object parameter );

		object Execute( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IParameterAware
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( (TParameter)parameter );

		public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
	}

	public class CommandAdapter : IParameterAware
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

	public class CommandAdapter<T> : IParameterAware
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

	public class ParameterWorkflow : IParameterAware
	{
		readonly IParameterWorkflowState state;
		readonly IsValid specification;
		readonly Execute execute;
		
		public ParameterWorkflow( IParameterWorkflowState state, IsValid specification, Execute execute )
		{
			this.state = state;
			this.specification = specification;
			this.execute = execute;
		}

		public bool IsValid( object parameter )
		{
			var result = specification( parameter );
			var valid = state.IsValidated( parameter );
			var isValid = result && !valid && !state.IsActive( parameter );
			state.Validate( parameter, isValid );
			return result;
		}

		bool AsActive( object parameter )
		{
			using ( new Assignment( state.Activate, parameter ).Configured( false ) )
			{
				return IsValid( parameter );
			}
		}

		object AsValid( object parameter )
		{
			using ( new Assignment( state.Validate, parameter ).Configured( false ) )
			{
				return execute( parameter );
			}
		}

		public object Execute( object parameter )
		{
			var result = Check( parameter ) || AsActive( parameter ) ? AsValid( parameter ) : null;
			return result;
		}

		bool Check( object parameter )
		{
			var result = state.IsValidated( parameter );
			if ( result )
			{
				state.Validate( parameter, false );
			}
			return result;
		}
	}
}
