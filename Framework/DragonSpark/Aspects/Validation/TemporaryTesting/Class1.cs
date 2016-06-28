using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation.TemporaryTesting
{
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

	[ApplyAutoValidation]
	class ValidatedCommand : CommandBase<object>
	{
		public ValidatedCommand() : base( new OnlyOnceSpecification() ) {}

		public bool Executed { get; private set; }

		public void Reset() => Executed = false;

		public override void Execute( object parameter ) => Executed = true;
	}
}
