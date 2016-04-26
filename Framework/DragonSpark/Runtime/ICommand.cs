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

		protected override void OnExecute( object p ) => command.Value.ExecuteWith( parameter.Value );

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

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : base( specification )
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

		public override bool CanExecute( T parameter ) => inner.CanExecute( parameter );

		protected override void OnExecute( T parameter ) => inner.Execute( parameter );
	}

	public class Specification<TParameter> : DecoratedSpecification<TParameter>
	{
		public static Specification<TParameter> Instance { get; } = new Specification<TParameter>();

		Specification() : base( NullSpecification.NotNull ) {}
	}

	public abstract class DecoratedCommand<TFrom, TTo> : CommandBase<TFrom>
	{
		readonly Func<TFrom, TTo> transform;
		readonly ICommand<TTo> inner;

		protected DecoratedCommand( Func<TFrom, TTo> transform, ICommand<TTo> inner )
		{
			this.transform = transform;
			this.inner = inner;
		}

		protected DecoratedCommand( ISpecification<TFrom> specification ) : base( specification ) {}

		protected override void OnExecute( TFrom parameter ) => inner.ExecuteWith( transform( parameter ) );
	}

	public abstract class CommandBase<TParameter> : ICommand<TParameter>
	{
		readonly ISpecification<TParameter> specification;

		public event EventHandler CanExecuteChanged = delegate {};

		protected CommandBase() : this( Specification<TParameter>.Instance ) {}

		protected CommandBase( ISpecification<TParameter> specification )
		{
			this.specification = specification;
		}

		public void Update() => OnUpdate();

		protected virtual void OnUpdate() => CanExecuteChanged( this, EventArgs.Empty );

		public virtual bool CanExecute( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		public void Execute( TParameter parameter ) => OnExecute( parameter );

		protected abstract void OnExecute( TParameter parameter );

		bool ICommand.CanExecute( object parameter ) => CanExecute( parameter.As<TParameter>() );

		void ICommand.Execute( object parameter ) => Execute( parameter.As<TParameter>() );
	}
}