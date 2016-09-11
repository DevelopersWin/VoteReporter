using System;
using DragonSpark.Extensions;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Extensions
{
	public static class Defaults
	{
		public static Func<Type, IExtension> PolicySource { get; } = Activator.Default.Get<IExtension>;
	}
}