using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Patterns.Contracts;
using PostSharp.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public FixedCommand( [Required] ICommand command, [Required] object parameter ) : this( command.Self, parameter.Self ) {}

		public FixedCommand( [Required] Func<ICommand> command, [Required] Func<object> parameter ) : base( Specifications.Specifications.Always )
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
		public DelegatedCommand( Action action ) : base( o => action(), Specifications.Specifications.Always ) {}
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
		public DecoratedCommand( [Required] ICommand<T> inner ) : this( inner, Coercer<T>.Instance ) {}
		public DecoratedCommand( [Required] ICommand<T> inner, ICoercer<T> coercer ) : base( inner.Execute, coercer, new DelegatedSpecification<T>( inner.CanExecute ) ) {}
	}

	public abstract class CommandBase<T> : ICommand<T>
	{
		readonly ICoercer<T> coercer;
		readonly ISpecification<T> specification;
		public event EventHandler CanExecuteChanged = delegate { };

		protected CommandBase() : this( Coercer<T>.Instance ) {}

		protected CommandBase( [Required] ICoercer<T> coercer ) : this( coercer, Specifications<T>.NotNull ) {}

		protected CommandBase( [Required] ISpecification<T> specification ) : this( Coercer<T>.Instance, specification ) {}

		protected CommandBase( [Required] ICoercer<T> coercer, [Required] ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public void Update() => OnUpdate();

		protected virtual void OnUpdate() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( object parameter ) => specification.IsSatisfiedBy( parameter );

		[ValidatedBy( nameof(CanExecute) )]
		void ICommand.Execute( object parameter ) => OnExecute( coercer.Coerce( parameter ) );

		public virtual bool CanExecute( T parameter ) => specification.IsSatisfiedBy( parameter );

		[ValidatedBy( nameof(CanExecute) )]
		public void Execute( T parameter ) => OnExecute( parameter );

		protected abstract void OnExecute( T parameter );
	}
}