﻿using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	public class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		readonly static Func<object, Implementation> Factory = new AspectFactory<Implementation>( controller => new Implementation( controller ) ).Get;

		public AutoValidationExecuteAspect() : base( Factory ) {}

		sealed class Implementation : AutoValidationExecuteAspect
		{
			readonly IAutoValidationController controller;
			public Implementation( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = controller.Execute( args.Arguments[0], args.GetReturnValue );
		}
	}
}