using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
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

		public FixedCommand( [Required]Func<ICommand> command, [Required]Func<object> parameter ) : base( Specifications<object>.Always )
		{
			this.command = new Lazy<ICommand>( command );
			this.parameter = new Lazy<object>( parameter );
		}

		protected override void OnExecute( object p ) => command.Value.Execute( parameter.Value );

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

	public class DelegatedCommand : DelegatedCommand<object>
	{
		public DelegatedCommand( Action action ) : base( o => action(), Specifications<object>.Always ) {}
	}

	public class DelegatedCommand<T> : CommandBase<T>
	{
		readonly Action<T> command;

		public DelegatedCommand( Action<T> command ) : this( command, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, ISpecification<T> specification ) : this( command, Coercer<T>.Instance, specification ) {}

		public DelegatedCommand( Action<T> command, ICoercer<T> coercer ) : this( command, coercer, Specifications<T>.Always ) {}

		public DelegatedCommand( Action<T> command, ICoercer<T> coercer, ISpecification<T> specification ) : base( coercer, specification )
		{
			this.command = command;
		}

		protected override void OnExecute( T parameter ) => command( parameter );
	}

	public class BoxedCommand<T> : CommandBase<T>
	{
		readonly ICommand command;
		readonly Func<T, object> box;

		public BoxedCommand( ICommand command ) : this( command, Default<T>.Boxed ) {}

		public BoxedCommand( ICommand command, Func<T, object> box )
		{
			this.command = command;
			this.box = box;
		}

		protected override void OnExecute( T parameter ) => command.Execute( box( parameter ) );
	}

	public class DecoratedCommand<T> : DelegatedCommand<T>
	{
		public DecoratedCommand( [Required]ICommand<T> inner ) : this( inner, Coercer<T>.Instance ) {}
		public DecoratedCommand( [Required]ICommand<T> inner, ICoercer<T> coercer ) : base( inner.Execute, coercer, new DelegatedSpecification<T>( inner.CanExecute ) ) {}
	}

	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate {};

		protected CommandBase() : this( Coercer<T>.Instance ) {}

		protected CommandBase( [Required]ICoercer<T> coercer ) : this( coercer, Specifications<T>.NotNull ) {}

		protected CommandBase( [Required]ISpecification<T> specification ) : this( Coercer<T>.Instance, specification ) {}

		protected CommandBase( [Required]ICoercer<T> coercer, [Required]ISpecification<T> specification ) : this( new ParameterSupport<T>( specification, coercer ) ) {}

		CommandBase( ParameterSupport<T> support )
		{
			Support = support;
		}

		protected ParameterSupport<T> Support { get; }

		/*protected ISpecification<T> Specification => support.Specification;
		protected ICoercer<T> Coercer => support.Coercer;*/

		public void Update() => OnUpdate();

		protected virtual void OnUpdate() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( object parameter ) => Support.IsValid( parameter );

		void ICommand.Execute( object parameter ) => Coerce( parameter );
		
		public virtual bool CanExecute( T parameter ) => Support.IsValid( parameter );

		public void Execute( T parameter ) => Coerce( parameter );

		protected abstract void OnExecute( T parameter );

		void Coerce( object parameter ) => Support.Coerce( parameter, OnExecute );
	}
}