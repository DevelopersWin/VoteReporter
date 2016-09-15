using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public sealed class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		/*readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Get;

		public AutoValidationExecuteAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationExecuteAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => ;
		}*/
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = args.Instance as IAutoValidationController;
			if ( controller != null )
			{
				args.ReturnValue = controller.Execute( args.Arguments[0], new Invocation( args.GetReturnValue ) ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
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