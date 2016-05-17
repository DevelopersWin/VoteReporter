using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	public abstract class ParameterWorkflowBinder<T, TParameter, TResult>
	{
		
	}

	public interface IParameterWorkflowState
	{
		void Activate( object parameter, bool on );

		void Validate( object parameter, bool on );

		bool IsActive( object parameter );

		bool IsValidated( object parameter );
	}

	class StateContext : IDisposable
	{
		readonly Action<object, bool> command;
		readonly object parameter;

		public StateContext( Action<object, bool> command, object parameter )
		{
			this.command = command;
			this.parameter = parameter;

			command( parameter, true );
		}

		public void Dispose() => command( parameter, false );
	}

	public class ParameterWorkflowState : IParameterWorkflowState
	{
		public static ParameterWorkflowState Instance { get; } = new ParameterWorkflowState();

		public void Activate( object parameter, bool on ) => new Active( this, parameter ).Assign( on );

		public bool IsActive( object parameter ) => new Active( this, parameter ).Value;

		public void Validate( object parameter, bool on ) => new Valid( this, parameter ).Assign( on );

		public bool IsValidated( object parameter ) => new Valid( this, parameter ).Value;

		class Valid : ThreadAmbientStore<bool>
		{
			public Valid( object instance, object parameter ) : base( KeyFactory.Instance.ToString( instance, parameter ) ) {}
		}

		class Active : ThreadAmbientStore<bool>
		{
			public Active( object instance, object parameter ) : base( KeyFactory.Instance.ToString( instance, parameter ) ) {}
		}
	}

	public class CommandWorkflow<T> : ParameterWorkflow<T, object>
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
	}

	public class FactoryWorkflow : ParameterWorkflow<object, object>
	{
		public FactoryWorkflow( IFactoryWithParameter instance ) : base( instance.CanCreate, instance.Create ) {}
	}

	public class ParameterWorkflow<TParameter, TResult>
	{
		readonly IParameterWorkflowState state;
		readonly Func<TParameter, bool> specification;
		readonly Func<TParameter, TResult> factory;
		readonly TResult defaultValue;

		public ParameterWorkflow( Func<TParameter, bool> specification, Action<TParameter> action ) : this( specification, action.ToFactory<TParameter, TResult>() ) {}

		public ParameterWorkflow( Func<TParameter, bool> specification, Func<TParameter, TResult> factory ) : this( ParameterWorkflowState.Instance, specification, factory, Default<TResult>.Item ) {}

		public ParameterWorkflow( IParameterWorkflowState state, Func<TParameter, bool> specification, Func<TParameter, TResult> factory, TResult defaultValue )
		{
			this.state = state;
			this.specification = specification;
			this.factory = factory;
			this.defaultValue = defaultValue;
		}

		public bool IsValid( TParameter parameter )
		{
			var result = specification( parameter );
			var valid = state.IsValidated( parameter );
			var isValid = result && !valid && !state.IsActive( parameter );
			state.Validate( parameter, isValid );
			return result;
		}

		bool AsActive( TParameter parameter )
		{
			using ( new StateContext( state.Activate, parameter ) )
			{
				return IsValid( parameter );
			}
		}

		TResult AsValid( TParameter parameter )
		{
			using ( new StateContext( state.Validate, parameter ) )
			{
				return factory( parameter );
			}
		}

		public TResult Apply( TParameter parameter )
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
