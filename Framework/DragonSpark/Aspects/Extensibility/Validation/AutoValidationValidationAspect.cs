using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public class AutoValidationValidationAspect : AutoValidationAspectBase
	{
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Get;
		public AutoValidationValidationAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationValidationAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args )
			{
				var parameter = args.Arguments[0];
				args.ReturnValue = controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, args.GetReturnValue<bool>() );
			}
		}
	}
}