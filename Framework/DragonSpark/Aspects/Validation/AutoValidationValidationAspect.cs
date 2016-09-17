using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var active = args.Instance as IActive;
			if ( active == null || !active.Get() )
			{
				var controller = args.Instance as IAutoValidationController;
				if ( controller != null )
				{
					var parameter = args.Arguments[0];
					args.ReturnValue = controller.Handles( parameter ) || controller.Marked( parameter, args.GetReturnValue<bool>() );
					return;
				}
			}
			base.OnInvoke( args );
		}
	}
}