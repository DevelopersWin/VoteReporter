using DragonSpark.Aspects.Adapters;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = args.Instance as IAutoValidationController;
			if ( controller != null && !controller.IsActive )
			{
				args.ReturnValue = controller.Execute( args.Arguments[0], new InvocationAdapter( args.GetReturnValue ) ) ?? args.ReturnValue;
			}
			else
			{
				args.Proceed();
			}
		}
	}
}