using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using DragonSpark.Activation.Sources;

namespace DragonSpark.Runtime
{
	public interface ICommand<in TParameter> : ICommand
	{
		bool CanExecute( TParameter parameter );

		void Execute( TParameter parameter );

		void Update();
	}

	public class AssignCommand<T> : DisposingCommand<T>
	{
		readonly IAssignable<T> assignable;
		readonly T current;

		public AssignCommand( IAssignableSource<T> store ) : this( store, store ) {}

		public AssignCommand( IAssignable<T> assignable, ISource<T> store ) : this( assignable, store.Get() ) {}

		public AssignCommand( IAssignable<T> assignable, [Optional]T current )
		{
			this.assignable = assignable;
			this.current = current;
		}

		public override void Execute( T parameter ) => assignable.Assign( parameter );

		protected override void OnDispose()
		{
			assignable.TryDispose();
			assignable.Assign( current );
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

		public FixedCommand( ICommand<T> command, T parameter ) : this( command, parameter, Specifications.Specifications.Always ) {}

		public FixedCommand( ICommand<T> command, T parameter, ISpecification<object> specification ) : base( specification )
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

	public class DecoratedCommand : DecoratedCommand<object>
	{
		public DecoratedCommand( ICommand<object> inner ) : base( inner ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer ) : base( inner, coercer ) {}
		public DecoratedCommand( ICommand<object> inner, ISpecification<object> specification ) : base( inner, specification ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer, ISpecification<object> specification ) : base( inner, coercer, specification ) {}
	}

	public class DecoratedCommand<T> : DelegatedCommand<T>
	{
		public DecoratedCommand( ICommand<T> inner ) : this( inner, Defaults<T>.Coercer ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer ) : this( inner, coercer, inner.ToSpecification() ) {}
		public DecoratedCommand( ICommand<T> inner, ISpecification<T> specification ) : this( inner, Defaults<T>.Coercer, specification ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer, ISpecification<T> specification ) : base( inner.ToDelegate(), coercer, specification ) {}
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

	public class ServiceCoercer<T> : CoercerBase<T>
	{
		public static ServiceCoercer<T> Instance { get; } = new ServiceCoercer<T>();
		ServiceCoercer() : this( GlobalServiceProvider.GetService<object> ) {}

		readonly ServiceSource source;

		public ServiceCoercer( ServiceSource source )
		{
			this.source = source;
		}

		protected override T PerformCoercion( object parameter ) => (T)source( typeof(T) );
	}

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

		bool ICommand.CanExecute( object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );

		void ICommand.Execute( object parameter ) => Execute( coercer( parameter ) );

		public virtual bool CanExecute( T parameter ) => specification.IsSatisfiedBy( parameter );

		public abstract void Execute( T parameter );
	}
}