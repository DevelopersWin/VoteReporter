using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

		public AssignValueCommand( IWritableStore<T> store ) : this( store, store.Value ) {}

		public AssignValueCommand( IWritableStore<T> store, [Optional]T current )
		{
			this.store = store;
			this.current = current;
		}

		public override void Execute( T parameter ) => store.Assign( parameter );

		protected override void OnDispose()
		{
			store.TryDispose();
			store.Assign( current );
			base.OnDispose();
		}
	}

	/*public class FixedCommand : FixedCommand<object>
	{
		public FixedCommand( ICommand<object> command, object parameter ) : base( command, parameter ) {}
	}*/


	public class FixedCommand<T> : DisposingCommand<object>
	{
		readonly ICommand<T> command;
		readonly T parameter;
		
		public FixedCommand( ICommand<T> command, T parameter )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override void Execute( object p ) => command.Execute( parameter );

		protected override void OnDispose() => command.TryDispose();
	}

	public class AddItemCommand<T> : CommandBase<T>
	{
		readonly IList<T> list;

		public AddItemCommand( [Required] IList<T> list )
		{
			this.list = list;
		}

		public override void Execute( T parameter ) => list.Add( parameter );
	}

	public class AddItemCommand : CommandBase<object>
	{
		readonly IList list;

		public AddItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Add( parameter );
	}

	public class RemoveItemCommand : CommandBase<object>
	{
		readonly IList list;

		public RemoveItemCommand( [Required] IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Remove( parameter );
	}

	public abstract class DisposingCommand<T> : CommandBase<T>, IDisposable
	{
		readonly Action onDispose;

		protected DisposingCommand() : this( Specifications<T>.Assigned ) {}

		protected DisposingCommand( ISpecification<T> specification ) : base( specification )
		{
			onDispose = OnDispose;
		}

		~DisposingCommand()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( onDispose );

		protected virtual void OnDispose() {}
	}

	/*public class DelegatedCommand : DelegatedCommand<object>
	{
		public DelegatedCommand( Action action ) : base( o => action(), Specifications.Specifications.Always ) {}
	}*/

	// [AutoValidation( false )]
	public class DelegatedCommand<T> : CommandBase<T>
	{
		readonly Action<T> command;

		public DelegatedCommand( Action<T> command ) : this( command, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : this( command, Defaults<T>.Coercer, specification ) {}

		public DelegatedCommand( Action<T> command, Coerce<T> coercer ) : this( command, coercer, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, Coerce<T> coercer, ISpecification<T> specification ) : base( coercer, specification )
		{
			this.command = command;
		}

		public override void Execute( T parameter ) => command( parameter );
	}

	public class ProjectedCommand<T> : CommandBase<T>
	{
		readonly ICommand command;
		readonly Func<T, object> projection;

		public ProjectedCommand( ICommand command ) : this( command, Delegates<T>.Object ) {}

		public ProjectedCommand( ICommand command, Func<T, object> projection )
		{
			this.command = command;
			this.projection = projection;
		}

		public override void Execute( T parameter ) => command.Execute( projection( parameter ) );
	}

	public class DecoratedCommand<T> : DelegatedCommand<T>
	{
		public DecoratedCommand( [Required] ICommand<T> inner ) : this( inner, Defaults<T>.Coercer ) {}
		public DecoratedCommand( [Required] ICommand<T> inner, Coerce<T> coercer ) : base( inner.ToDelegate(), coercer, inner.ToSpecification() ) {}
	}

	/*public class AutoValidatingCommand<T> : ICommand<T>
	{
		readonly IAutoValidationController controller;
		readonly ICommand<T> inner;

		public AutoValidatingCommand( ICommand<T> inner ) : this( new AutoValidationController( new CommandAdapter<T>( inner ) ), inner ) {}

		public AutoValidatingCommand( IAutoValidationController controller, ICommand<T> inner )
		{
			this.controller = controller;
			this.inner = inner;
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add { inner.CanExecuteChanged += value; }
			remove { inner.CanExecuteChanged -= value; }
		}

		public bool CanExecute( object parameter ) => controller.IsValid( parameter );

		public void Execute( object parameter ) => controller.Execute( parameter );

		public bool CanExecute( T parameter ) => controller.IsValid( parameter );

		public void Execute( T parameter ) => controller.Execute( parameter );
		public void Update() => inner.Update();
	}*/

	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate { };
		readonly Coerce<T> coercer;
		readonly ISpecification<T> specification;

		protected CommandBase() : this( Defaults<T>.Coercer ) {}

		protected CommandBase( [Required] Coerce<T> coercer ) : this( coercer, Specifications<T>.Assigned ) {}

		protected CommandBase( [Required] ISpecification<T> specification ) : this( Defaults<T>.Coercer, specification ) {}

		protected CommandBase( [Required] Coerce<T> coercer, [Required] ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( object parameter ) => specification.IsSatisfiedBy( parameter );

		void ICommand.Execute( object parameter ) => Execute( coercer( parameter ) );

		public virtual bool CanExecute( T parameter ) => specification.IsSatisfiedBy( parameter );

		public abstract void Execute( T parameter );
	}
}