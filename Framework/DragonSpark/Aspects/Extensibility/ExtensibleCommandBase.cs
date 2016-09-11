using DragonSpark.Commands;
using PostSharp.Extensibility;
using System;
using System.Windows.Input;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Extensibility
{
	public abstract class ExtensibleCommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate {};

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		[ExtensionPoint]
		bool ICommand.CanExecute( object parameter ) => parameter is T && IsSatisfiedBy( (T)parameter );
		[ExtensionPoint]
		void ICommand.Execute( object parameter ) {}

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict )]
		public virtual bool IsSatisfiedBy( T parameter ) => parameter.IsAssigned();

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict, AttributeTargetMemberAttributes = MulticastAttributes.NonAbstract )]
		public abstract void Execute( T parameter );
	}
}
