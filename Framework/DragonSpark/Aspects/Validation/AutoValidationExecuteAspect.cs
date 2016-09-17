using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AutoValidationExecuteAspect : AutoValidationAspectBase
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
					active?.Assign( true );
					args.ReturnValue = controller.Execute( args.Arguments[0], new Invocation( args.GetReturnValue ) ) ?? args.ReturnValue;
					active?.Assign( false );
					return;
				}
			}
			args.Proceed();
		}

		sealed class Invocation : IInvocation
		{
			readonly Func<object> factory;

			public Invocation( Func<object> factory )
			{
				this.factory = factory;
			}

			public object Invoke( object parameter ) => factory();
		}
	}
}