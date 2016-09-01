using System;
using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Diagnostics.Logging
{
	public static class Configuration
	{
		public static IParameterizedScope<string, Func<MethodBase, IDisposable>> TimedOperationFactory { get; } = new ParameterizedScope<string, Func<MethodBase, IDisposable>>( TimedOperationFactorySource.Default.ToSourceDelegate().GlobalCache() );
	}
}