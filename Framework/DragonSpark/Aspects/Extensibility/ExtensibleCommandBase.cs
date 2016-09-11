using DragonSpark.Commands;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Aspects.Extensibility
{
	public abstract class ExtensibleCommandBase<T> : ICommand<T>, IExtensionProvider
	{
		public event EventHandler CanExecuteChanged = delegate {};

		public virtual void Update() => CanExecuteChanged( this, EventArgs.Empty );

		[ExtensionPoint]
		bool ICommand.CanExecute( object parameter ) => false;
		[ExtensionPoint]
		void ICommand.Execute( object parameter ) {}

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict )]
		public virtual bool IsSatisfiedBy( T parameter ) => true;

		[ExtensionPoint( AttributeInheritance =  MulticastInheritance.Strict, AttributeTargetMemberAttributes = MulticastAttributes.NonAbstract )]
		public abstract void Execute( T parameter );

		public virtual IEnumerable<IExtension> GetExtensions()
		{
			yield break;
		}
	}
}
