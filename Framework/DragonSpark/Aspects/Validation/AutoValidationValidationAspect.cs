using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var instance = args.Instance;
			var controller = instance as IAutoValidationController;
			if ( controller != null )
			{
				var active = instance as IActive;
				var allowed = active == null || !active.Get();
				if ( allowed )
				{
					var parameter = args.Arguments[0];
					args.ReturnValue = controller.Handles( parameter ) || controller.Marked( parameter, args.GetReturnValue<bool>() );
					return;
				}
			}
			
			args.Proceed();
		}
	}
}