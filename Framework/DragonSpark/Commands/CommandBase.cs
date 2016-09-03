using DragonSpark.Sources.Parameterized;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public abstract class CommandBase<T> : ICommand<T>
	{
		public event EventHandler CanExecuteChanged = delegate { };

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		bool ICommand.CanExecute( [Optional]object parameter ) => IsSatisfiedBy( Defaults<T>.Coercer( parameter ) );
		void ICommand.Execute( [Optional]object parameter ) => Execute( Defaults<T>.Coercer( parameter ) );

		public virtual bool IsSatisfiedBy( [Optional]T parameter ) => true;
		public abstract void Execute( T parameter );
	}
}