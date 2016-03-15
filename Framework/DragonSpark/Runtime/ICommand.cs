using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Windows.Input;
using DragonSpark.Runtime.Specifications;
using Activator = DragonSpark.Activation.Activator;

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
		readonly IWritableValue<T> value;
		readonly T current;

		public AssignValueCommand( [Required]IWritableValue<T> value ) : this( value, value.Item ) {}

		public AssignValueCommand( [Required]IWritableValue<T> value, T current )
		{
			this.value = value;
			this.current = current;
		}

		protected override void OnExecute( T parameter ) => value.Assign( parameter );

		protected override void OnDispose()
		{
			value.TryDispose();
			value.Assign( current );
			base.OnDispose();
		}
	}

	public class FixedCommand : DisposingCommand<object>
	{
		readonly ICommand command;
		readonly Func<object> parameter;

		public FixedCommand( [Required]ICommand command, [Required]object parameter ) : this( command, () => parameter ) {}

		public FixedCommand( [Required]ICommand command, [Required]Func<object> parameter )
		{
			this.command = command;
			this.parameter = parameter;
		}

		protected override void OnExecute( object p ) => command.ExecuteWith( parameter() );

		protected override void OnDispose()
		{
			base.OnDispose();
			command.TryDispose();
		}
	}

	/*public class DisposingCompositeCommand : DisposingCompositeCommand<object>
	{
		public DisposingCompositeCommand( params ICommand<object>[] commands ) : base( commands ) {}
	}

	public class DisposingCompositeCommand<TParameter> : DisposingCommand<TParameter>
	{
		readonly CompositeCommand<TParameter> body;

		public DisposingCompositeCommand( [Required]params ICommand<TParameter>[] commands )
		{
			body = new CompositeCommand<TParameter>( commands );
		}

		protected override void OnExecute( TParameter parameter ) => body.ExecuteWith( parameter );
	}*/

	public abstract class DisposingCommand<TParameter> : Command<TParameter>, IDisposable
	{
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

	public class DeferredCommand<TCommand, T> : Command<T> where TCommand : ICommand<T>
	{
		readonly Func<TCommand> factory;

		public DeferredCommand() : this( Activator.Activate<TCommand> ) {}

		public DeferredCommand( [Required]Func<TCommand> factory )
		{
			this.factory = factory;
		}

		protected override void OnExecute( T parameter ) => factory().ExecuteWith( parameter );
	}

	public class DecoratedCommand<T> : Command<T>
	{
		readonly ICommand inner;

		public DecoratedCommand( [Required]ICommand inner )
		{
			this.inner = inner;
		}

		public override bool CanExecute( T parameter ) => inner.CanExecute( parameter );

		protected override void OnExecute( T parameter ) => inner.Execute( parameter );
	}

	public abstract class Command<TParameter> : Command<TParameter, ISpecification<TParameter>>
	{
		protected Command() : this( Specification<TParameter>.Instance ) {}

		protected Command( ISpecification<TParameter> specification ) : base( specification ) {}
	}

	public class Specification<TParameter> : WrappedSpecification<TParameter>
	{
		public static Specification<TParameter> Instance { get; } = new Specification<TParameter>();

		Specification() : base( NullSpecification.NotNull ) {}
	}

	public abstract class Command<TParameter, TSpecification> : ICommand<TParameter> where TSpecification : ISpecification<TParameter>
	{
		readonly TSpecification specification;

		public event EventHandler CanExecuteChanged = delegate {};

		protected Command( TSpecification specification )
		{
			this.specification = specification;
		}

		public void Update() => OnUpdate();

		protected virtual void OnUpdate() => CanExecuteChanged( this, EventArgs.Empty );

		public virtual bool CanExecute( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		public void Execute( TParameter parameter ) => OnExecute( parameter );

		protected abstract void OnExecute( TParameter parameter );

		bool ICommand.CanExecute( object parameter ) => parameter.AsTo<TParameter, bool>( CanExecute );

		void ICommand.Execute( object parameter ) => parameter.AsValid<TParameter>( Execute, $"'{GetType().FullName}' expects a '{typeof( TParameter ).FullName}' object to execute." );
	}
}