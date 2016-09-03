using DragonSpark.Specifications;
using System;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate {};

		readonly ICoercer<T> coercer;
		readonly ISpecification<T> specification;

		protected CommandBase() : this( Specifications<T>.Assigned ) {}
		protected CommandBase( ISpecification<T> specification ) : this( Coercer<T>.Default, specification ) {}
		protected CommandBase( ICoercer<T> coercer, ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( object parameter ) => IsSatisfiedBy( coercer.Coerce( parameter ) );
		void ICommand.Execute( object parameter ) => Execute( coercer.Coerce( parameter ) );

		public bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );
		public abstract void Execute( T parameter );
	}
}