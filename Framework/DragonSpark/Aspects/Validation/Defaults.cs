using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	static class Defaults
	{
		public static IEnumerable<KeyValuePair<TypeAdapter, Func<object, IParameterValidationAdapter>>> Factories { get; } = new Dictionary<TypeAdapter, Func<object, IParameterValidationAdapter>>
																								   {
																									   { ParameterizedSourceDefinition.Default.DeclaringType.Adapt(), ParameterizedSourceAdapterFactory.Default.Get },
																									   { GenericCommandDefinition.Default.DeclaringType.Adapt(), GenericCommandAdapterFactory.Default.Get },
																									   { CommandDefinition.Default.DeclaringType.Adapt(), CommandAdapterFactory.Default.Get }
																								   };

		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Default.Get;
	}
}