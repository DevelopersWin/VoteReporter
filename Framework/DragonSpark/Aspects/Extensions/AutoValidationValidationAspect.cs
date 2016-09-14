using DragonSpark.Aspects.Extensibility.Validation;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Extensions
{
	public sealed class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = args.Instance as IAutoValidationController;
			if ( controller != null )
			{
				var parameter = args.Arguments[0];
				args.ReturnValue = controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, args.GetReturnValue<bool>() );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}