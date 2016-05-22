using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
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

		public void Activate( object parameter, bool on ) => active.SetEnabled( parameter, @on );

		public bool IsActive( object parameter ) => active.IsEnabled( parameter );

		public void Validate( object parameter, bool on ) => valid.SetEnabled( parameter, @on );

		public bool IsValidated( object parameter ) => valid.IsEnabled( parameter );

		class Container
		{
			readonly ThreadLocal<ISet<object>> store = new ThreadLocal<ISet<object>>( () => new HashSet<object>() );
			// readonly ISet<object> store = new HashSet<object>();

			public void SetEnabled( object parameter, bool on )
			{
				if ( on )
				{
					store.Value.Add( parameter );
				}
				else
				{
					store.Value.Remove( parameter );
				}
			}

			public bool IsEnabled( object parameter ) => store.Value.Contains( parameter );
		}

		/*string Key<T>( object parameter ) => KeyFactory.Instance.ToString( typeof(T), instance, parameter );

		class Valid : ThreadAmbientStore<bool>
		{
			public Valid( string key ) : base( key ) {}
		}

		class Active : ThreadAmbientStore<bool>
		{
			public Active( string key ) : base( key ) {}
		}*/
	}

	public interface IParameterAware
	{
		bool IsAllowed( object parameter );

		object Execute( object parameter );
	}

	public class FactoryWithParameterAware : IParameterAware
	{
		readonly IFactoryWithParameter inner;
		public FactoryWithParameterAware( IFactoryWithParameter inner )
		{
			this.inner = inner;
		}

		public bool IsAllowed( object parameter ) => inner.CanCreate( parameter );

		public object Execute( object parameter ) => inner.Create( parameter );
	}

	public class FactoryParameterAware<TParameter, TResult> : IParameterAware
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryParameterAware( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsAllowed( object parameter ) => inner.CanCreate( (TParameter)parameter );

		public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
	}

	public class CommandParameterAware : IParameterAware
	{
		readonly ICommand inner;
		public CommandParameterAware( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsAllowed( object parameter ) => inner.CanExecute( parameter );

		public object Execute( object parameter )
		{
			inner.Execute( parameter );
			return null;
		}
	}

	public class CommandParameterAware<T> : IParameterAware
	{
		readonly ICommand<T> inner;
		public CommandParameterAware( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsAllowed( object parameter ) => inner.CanExecute( (T)parameter );

		public object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}
	}

	public class ParameterWorkflow : ParameterWorkflow<object, object>, IParameterAware
	{
		public ParameterWorkflow( IParameterWorkflowState state, IParameterAware aware ) : this ( state, aware.IsAllowed, aware.Execute ) {}

		ParameterWorkflow( IParameterWorkflowState state, Func<object, bool> specification, Func<object, object> execute ) : base( state, specification, execute ) {}
	}

	public class ParameterWorkflow<TParameter, TResult>
	{
		readonly IParameterWorkflowState state;
		readonly Func<TParameter, bool> specification;
		readonly Func<TParameter, TResult> execute;
		readonly TResult defaultValue;

		// public ParameterWorkflow( IParameterWorkflowState state, Func<TParameter, bool> specification, Action<TParameter> action ) : this( state, specification, action.ToFactory<TParameter, TResult>() ) {}

		public ParameterWorkflow( IParameterWorkflowState state, Func<TParameter, bool> specification, Func<TParameter, TResult> execute ) : this( state, specification, execute, Default<TResult>.Item ) {}

		public ParameterWorkflow( IParameterWorkflowState state, Func<TParameter, bool> specification, Func<TParameter, TResult> execute, TResult defaultValue )
		{
			this.state = state;
			this.specification = specification;
			this.execute = execute;
			this.defaultValue = defaultValue;
		}

		public bool IsAllowed( TParameter parameter )
		{
			var result = specification( parameter );
			var valid = state.IsValidated( parameter );
			var isValid = result && !valid && !state.IsActive( parameter );
			state.Validate( parameter, isValid );
			return result;
		}

		bool AsActive( TParameter parameter )
		{
			using ( new Assignment( state.Activate, parameter ).Configured( false ) )
			{
				return IsAllowed( parameter );
			}
		}

		TResult AsValid( TParameter parameter )
		{
			using ( new Assignment( state.Validate, parameter ).Configured( false ) )
			{
				return execute( parameter );
			}
		}

		public TResult Execute( TParameter parameter )
		{
			var result = Check( parameter ) || AsActive( parameter ) ? AsValid( parameter ) : defaultValue;
			return result;
		}

		bool Check( TParameter parameter )
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
