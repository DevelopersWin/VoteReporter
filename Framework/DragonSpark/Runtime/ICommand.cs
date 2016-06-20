using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
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

		public AssignValueCommand( [Required] IWritableStore<T> store ) : this( store, store.Value ) {}

		public AssignValueCommand( [Required] IWritableStore<T> store, T current )
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

	// [AutoValidation( false )]
	public class FixedCommand : DisposingCommand<object>
	{
		readonly Lazy<ICommand> command;
		readonly Lazy<object> parameter;

		public FixedCommand( [Required] ICommand command, [Required] object parameter ) : this( command.Self, parameter.Self ) {}

		public FixedCommand( [Required] Func<ICommand> command, [Required] Func<object> parameter ) : base( Specifications.Specifications.Always )
		{
			this.command = new Lazy<ICommand>( command );
			this.parameter = new Lazy<object>( parameter );
		}

		public override void Execute( object p ) => command.Value.Execute( parameter.Value );

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

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : this( command, Parameter<T>.Coercer, specification ) {}

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
		public DecoratedCommand( [Required] ICommand<T> inner ) : this( inner, Parameter<T>.Coercer ) {}
		public DecoratedCommand( [Required] ICommand<T> inner, Coerce<T> coercer ) : base( inner.ToDelegate(), coercer, inner.ToSpecification() ) {}
	}

	[ValidatedGenericCommand, ValidatedGenericCommand.Aspects]
	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate { };
		readonly Coerce<T> coercer;
		readonly ISpecification<T> specification;

		protected CommandBase() : this( Parameter<T>.Coercer ) {}

		protected CommandBase( [Required] Coerce<T> coercer ) : this( coercer, Specifications<T>.Assigned ) {}

		protected CommandBase( [Required] ISpecification<T> specification ) : this( Parameter<T>.Coercer, specification ) {}

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