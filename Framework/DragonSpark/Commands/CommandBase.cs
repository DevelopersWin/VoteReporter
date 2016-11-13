using DragonSpark.Aspects.Relay;
using System;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate {};

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		[SpecificationRelay]
		bool ICommand.CanExecute( object parameter ) => true;

		[CommandRelay]
		void ICommand.Execute( object parameter ) {}

		public virtual bool IsSatisfiedBy( T parameter ) => true;
		public abstract void Execute( T parameter );
	}
}