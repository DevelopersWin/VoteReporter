namespace DragonSpark.Aspects.Extensibility.Validation
{
	sealed class AutoValidationExecutor : IInvocation
	{
		readonly IAutoValidationController controller;
		readonly Active active;
		readonly IInvocation next;

		public AutoValidationExecutor( IAutoValidationController controller, IInvocation next, Active active )
		{
			this.controller = controller;
			this.active = active;
			this.next = next;
		}

		public object Invoke( object parameter )
		{
			active.IsActive = true;
			var result = controller.Execute( parameter, next );
			active.IsActive = false;
			return result;
		}
	}
}