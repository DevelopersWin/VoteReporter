using System;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectFactory<T> : ParameterizedSourceBase<T> where T : class, IAspect
	{
		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<IAutoValidationController, T> resultSource;

		public AspectFactory( Func<IAutoValidationController, T> resultSource ) : this( Defaults.ControllerSource, resultSource ) {}

		public AspectFactory( Func<object, IAutoValidationController> controllerSource, Func<IAutoValidationController, T> resultSource )
		{
			this.controllerSource = controllerSource;
			this.resultSource = resultSource;
		}

		public override T Get( object parameter ) => resultSource( controllerSource( parameter ) );
	}
}