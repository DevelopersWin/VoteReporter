using DragonSpark.Commands;
using DragonSpark.Runtime;
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	static class Defaults
	{
		public static Func<MethodLocator.Parameter, MethodInfo> Locator { get; } = MethodLocator.Default.Get;

		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Default.Get;

		public static ImmutableArray<IAspectProfile> AspectProfiles { get; } = 
			new IAspectProfile[]
			{
				// new AspectProfile( typeof(IValidatedParameterizedSource<,>), typeof(IParameterizedSource<,>), nameof(IParameterizedSource.Get) ),
				// new AspectProfile( typeof(IValidatedParameterizedSource), typeof(IParameterizedSource), nameof(IParameterizedSource.Get) ),
				new AspectProfile( new MethodDescriptor( typeof(ICommand<>), nameof(ICommand.Execute) ) ),
				new AspectProfile( new MethodDescriptor( typeof(ICommand), nameof(ICommand.Execute) ), new MethodDescriptor( typeof(ICommand), nameof(ICommand.CanExecute) ) )
			}.ToImmutableArray();
	}
}