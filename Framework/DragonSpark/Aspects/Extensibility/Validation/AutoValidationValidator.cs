namespace DragonSpark.Aspects.Extensibility.Validation
{
	sealed class AutoValidationValidator : InvocationBase<object, bool>
	{
		readonly IAutoValidationController controller;
		readonly IInvocation next;
		readonly Active active;

		public AutoValidationValidator( IAutoValidationController controller, IInvocation next, Active active )
		{
			this.controller = controller;
			this.next = next;
			this.active = active;
		}

		public override bool Invoke( object parameter ) => 
			active.IsActive ? (bool)next.Invoke( parameter ) : controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, (bool)next.Invoke( parameter ) );

		// public bool IsSatisfiedBy( object parameter ) => !active.IsActive;
	}
}
