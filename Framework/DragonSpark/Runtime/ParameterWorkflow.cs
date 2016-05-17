using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System;

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
		readonly object instance;

		public ParameterWorkflowState( object instance )
		{
			this.instance = instance;
		}

		public void Activate( object parameter, bool on ) => new Active( Key<Active>( parameter ) ).Assign( on );

		public bool IsActive( object parameter ) => new Active( Key<Active>( parameter ) ).Value;

		public void Validate( object parameter, bool on ) => new Valid( Key<Valid>( parameter ) ).Assign( on );

		public bool IsValidated( object parameter ) => new Valid( Key<Valid>( parameter ) ).Value;

		string Key<T>( object parameter ) => KeyFactory.Instance.ToString( typeof(T), instance, parameter );

		class Valid : ThreadAmbientStore<bool>
		{
			public Valid( string key ) : base( key ) {}
		}

		class Active : ThreadAmbientStore<bool>
		{
			public Active( string key ) : base( key ) {}
		}
	}

	/*public class CommandWorkflow<T> : ParameterWorkflow<T, object>
	{
		public CommandWorkflow( ICommand<T> command ) : base( command.CanExecute, new Action<T>( command.Execute ) ) {}
	}

	public class CommandWorkflow : ParameterWorkflow<object, object>
	{
		public CommandWorkflow( ICommand command ) : base( command.CanExecute, new Action<object>( command.Execute ) ) {}
	}

	public class FactoryWorkflow<TParameter, TResult> : ParameterWorkflow<TParameter, TResult>
	{
		public FactoryWorkflow( IFactory<TParameter, TResult> factory ) : base( factory.CanCreate, factory.Create ) {}
	}*/

	/*public class FactoryWorkflow : ParameterWorkflow
	{
		public FactoryWorkflow( IFactoryWithParameter instance ) : base( instance.CanCreate, instance.Create ) {}
	}

	public interface IParameterWorkflow
	{
		bool IsValid( object parameter );

		object Apply( object parameter );
	}*/

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

	public class ParameterWorkflow : ParameterWorkflow<object, object>, IParameterAware
	{
		public ParameterWorkflow( object target, Func<object, bool> specification, Action<object> action ) : base( target, specification, action ) {}

		public ParameterWorkflow( object target, IParameterAware aware ) : this ( target, aware.IsAllowed, aware.Execute ) {}

		public ParameterWorkflow( object target, Func<object, bool> specification, Func<object, object> factory ) : base( target, specification, factory ) {}
		// public ParameterWorkflow( IParameterWorkflowState state, Func<object, bool> specification, Func<object, object> factory, object defaultValue ) : base( state, specification, factory, defaultValue ) {}
	}

	public class ParameterWorkflow<TParameter, TResult>
	{
		readonly IParameterWorkflowState state;
		readonly Func<TParameter, bool> specification;
		readonly Func<TParameter, TResult> factory;
		readonly TResult defaultValue;

		public ParameterWorkflow( object target, Func<TParameter, bool> specification, Action<TParameter> action ) : this( target, specification, action.ToFactory<TParameter, TResult>() ) {}

		public ParameterWorkflow( object target, Func<TParameter, bool> specification, Func<TParameter, TResult> factory ) : this( new ParameterWorkflowState( target ), specification, factory, Default<TResult>.Item ) {}

		public ParameterWorkflow( IParameterWorkflowState state, Func<TParameter, bool> specification, Func<TParameter, TResult> factory, TResult defaultValue )
		{
			this.state = state;
			this.specification = specification;
			this.factory = factory;
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
			using ( new Assignment( state.Activate, parameter ) )
			{
				return IsAllowed( parameter );
			}
		}

		TResult AsValid( TParameter parameter )
		{
			using ( new Assignment( state.Validate, parameter ) )
			{
				return factory( parameter );
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
