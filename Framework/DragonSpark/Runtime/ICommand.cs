using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Runtime
{
	public interface ICommand<in TParameter> : ICommand
	{
		bool CanExecute( TParameter parameter );

		void Execute( TParameter parameter );

		void Update();
	}

	public class AssignValueCommand<T> : DisposingCommand<T>
	{
		readonly IWritableStore<T> store;
		readonly T current;

		public AssignValueCommand( [Required]IWritableStore<T> store ) : this( store, store.Value ) {}

		public AssignValueCommand( [Required]IWritableStore<T> store, T current )
		{
			this.store = store;
			this.current = current;
		}

		protected override void OnExecute( T parameter ) => store.Assign( parameter );

		protected override void OnDispose()
		{
			store.TryDispose();
			store.Assign( current );
			base.OnDispose();
		}
	}

	public class FixedCommand : DisposingCommand<object>
	{
		readonly Lazy<ICommand> command;
		readonly Lazy<object> parameter;

		public FixedCommand( [Required]ICommand command, [Required]object parameter ) : this( command.Self, parameter.Self ) {}

		public FixedCommand( [Required]Func<ICommand> command, [Required]Func<object> parameter ) : base( Common<object>.Always )
		{
			this.command = new Lazy<ICommand>( command );
			this.parameter = new Lazy<object>( parameter );
		}

		protected override void OnExecute( object p ) => command.Value.Executed( parameter.Value );

		protected override void OnDispose()
		{
			base.OnDispose();
			command.Value.TryDispose();
		}
	}

	public class AddItemCommand<T> : CommandBase<T>
	{
		readonly IList<T> list;

		public AddItemCommand( [Required] IList<T> list )
		{
			this.list = list;
		}

		protected override void OnExecute( T parameter ) => list.Add( parameter );
	}

	public class AddItemCommand : CommandBase<object>
	{
		readonly IList list;

		public AddItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnExecute( object parameter ) => list.Add( parameter );
	}

	public class RemoveItemCommand : CommandBase<object>
	{
		readonly IList list;

		public RemoveItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnExecute( object parameter ) => list.Remove( parameter );
	}

	public abstract class DisposingCommand<TParameter> : CommandBase<TParameter>, IDisposable
	{
		protected DisposingCommand() {}

		protected DisposingCommand( ISpecification<TParameter> specification ) : base( specification ) {}

		~DisposingCommand()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		protected virtual void OnDispose() {}
	}

	/*public class DeferredCommand<TCommand, T> : Command<T> where TCommand : ICommand<T>
	{
		readonly Func<TCommand> factory;

		public DeferredCommand() : this( Services.Get<TCommand> ) {}

		public DeferredCommand( [Required]Func<TCommand> factory )
		{
			this.factory = factory;
		}

		protected override void OnExecute( T parameter ) => factory().ExecuteWith( parameter );
	}*/

	public class DelegatedCommand : CommandBase<object>
	{
		readonly Action action;

		public DelegatedCommand( Action action )
		{
			this.action = action;
		}

		protected override void OnExecute( object parameter ) => action();
	}

	public class DelegatedCommand<T> : CommandBase<T>
	{
		readonly Action<T> command;

		public DelegatedCommand( Action<T> command ) : this( command, Specification<T>.Instance ) {}

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : this( command, specification, Coercer<T>.Instance ) {}

		public DelegatedCommand( Action<T> command, ICoercer<T> coercer ) : this( command, Specification<T>.Instance, coercer ) {}

		public DelegatedCommand( Action<T> command, ISpecification<T> specification, ICoercer<T> coercer ) : base( specification, coercer )
		{
			this.command = command;
		}

		protected override void OnExecute( T parameter ) => command( parameter );
	}

	public class DecoratedCommand<T> : CommandBase<T>
	{
		readonly ICommand<T> inner;

		public DecoratedCommand( [Required]ICommand<T> inner )
		{
			this.inner = inner;
		}

		protected override void OnExecute( T parameter ) => inner.Execute( parameter );
	}

	public abstract class DecoratedCommand<TFrom, TTo> : CommandBase<TTo>
	{
		readonly ICommand<TTo> inner;

		protected DecoratedCommand( Func<TFrom, TTo> projection, ICommand<TTo> inner ) : base( new Projector<TFrom, TTo>( projection ) )
		{
			this.inner = inner;
		}

		public override bool CanExecute( TTo parameter ) => inner.CanExecute( parameter );

		protected override void OnExecute( TTo parameter ) => inner.Execute( parameter );
	}

	public class Specification<TParameter> : DecoratedSpecification<TParameter>
	{
		public static Specification<TParameter> Instance { get; } = new Specification<TParameter>();

		Specification() : base( NullSpecification.NotNull ) {}
	}

	public abstract class CommandBase<T> : ICommand<T>
	{
		readonly ParameterSupport<T> support;

		public event EventHandler CanExecuteChanged = delegate {};

		protected CommandBase() : this( Coercer<T>.Instance ) {}

		protected CommandBase( [Required]ICoercer<T> coercer ) : this( Common<T>.Always, coercer ) {}

		protected CommandBase( [Required]ISpecification<T> specification ) : this( specification, Coercer<T>.Instance ) {}

		protected CommandBase( [Required]ISpecification<T> specification, [Required]ICoercer<T> coercer ) : this( new ParameterSupport<T>( specification, coercer ) ) {}

		CommandBase( ParameterSupport<T> support )
		{
			this.support = support;
		}

		public void Update() => OnUpdate();

		protected virtual void OnUpdate() => CanExecuteChanged( this, EventArgs.Empty );

		public virtual bool CanExecute( T parameter ) => support.IsValid( parameter );

		public void Execute( T parameter ) => OnExecute( parameter );

		protected abstract void OnExecute( T parameter );

		bool ICommand.CanExecute( object parameter ) => support.IsValid( parameter );

		public void Execute( object parameter ) => support.Coerce( parameter, Execute );
	}
}