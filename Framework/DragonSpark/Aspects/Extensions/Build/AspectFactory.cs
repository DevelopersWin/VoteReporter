using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Extensions.Build
{
	sealed class AspectFactory<T> : ParameterizedSourceBase<T> where T : class, IAspect
	{
		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<IAutoValidationController, T> resultSource;

		public AspectFactory( Func<IAutoValidationController, T> resultSource ) : this( Extensibility.Validation.Defaults.ControllerSource, resultSource ) {}

		public AspectFactory( Func<object, IAutoValidationController> controllerSource, Func<IAutoValidationController, T> resultSource )
		{
			this.controllerSource = controllerSource;
			this.resultSource = resultSource;
		}

		public override T Get( object parameter ) => resultSource( controllerSource( parameter ) );
	}
}