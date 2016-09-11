using System;
using DragonSpark.Extensions;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Extensibility
{
	public static class Defaults
	{
		public static Func<Type, IExtension> ExtensionSource { get; } = Activator.Default.Get<IExtension>;
	}
}