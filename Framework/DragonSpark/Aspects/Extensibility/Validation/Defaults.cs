using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	static class Defaults
	{
		public static Func<MethodLocator.Parameter, MethodInfo> Locator { get; } = MethodLocator.Default.Get;

		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Default.Get;
	}
}