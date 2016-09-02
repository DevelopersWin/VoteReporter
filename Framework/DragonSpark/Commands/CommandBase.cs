using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate { };
		readonly Coerce<T> coercer;
		readonly ISpecification<T> specification;

		protected CommandBase() : this( Defaults<T>.Coercer ) {}

		protected CommandBase( Coerce<T> coercer ) : this( coercer, Specifications<T>.Assigned ) {}

		protected CommandBase( ISpecification<T> specification ) : this( Defaults<T>.Coercer, specification ) {}

		protected CommandBase( Coerce<T> coercer, ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( [Optional]object parameter ) => Coerce( parameter );
		bool ISpecification.IsSatisfiedBy( [Optional]object parameter ) => Coerce( parameter );

		bool Coerce( [Optional]object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );
		public virtual bool IsSatisfiedBy( [Optional]T parameter ) => specification.IsSatisfiedBy( parameter );

		void ICommand.Execute( [Optional]object parameter ) => Execute( coercer( parameter ) );

		public abstract void Execute( T parameter );
	}
}