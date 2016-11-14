using DragonSpark.Activation;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Sources.Scopes;
using System;
using System.Reflection;

namespace DragonSpark.Diagnostics
{
	public sealed class TimedOperations : EqualityReferenceCache<string, Func<MethodBase, IDisposable>>
	{
		public static TimedOperations Default { get; } = new TimedOperations();
		TimedOperations() : base( Configuration.Implementation.GetDelegate ) {}

		public sealed class Configuration : ParameterizedScope<string, IParameterizedSource<MethodBase, IDisposable>>
		{
			public static Configuration Implementation { get; } = new Configuration();
			Configuration() : base( ParameterConstructor<string, TimedOperationFactory>.Default ) {}
		}
	}
}