using DragonSpark.Commands;
using DragonSpark.Specifications;
using PostSharp.Extensibility;
using System;
using System.Windows.Input;

namespace DragonSpark.Aspects.Extensibility
{
	public abstract class ExtensibleCommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate {};

		readonly ICoercer<T> coercer;
		readonly ISpecification<T> specification;

		protected ExtensibleCommandBase() : this( Specifications<T>.Assigned ) {}
		protected ExtensibleCommandBase( ISpecification<T> specification ) : this( Coercer<T>.Default, specification ) {}
		protected ExtensibleCommandBase( ICoercer<T> coercer, ISpecification<T> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		[ExtensionPoint]
		bool ICommand.CanExecute( object parameter ) => IsSatisfiedBy( coercer.Coerce( parameter ) );
		[ExtensionPoint]
		void ICommand.Execute( object parameter ) => Execute( coercer.Coerce( parameter ) );

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict )]
		public virtual bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict, AttributeTargetMemberAttributes = MulticastAttributes.NonAbstract )]
		public abstract void Execute( T parameter );
	}
}
