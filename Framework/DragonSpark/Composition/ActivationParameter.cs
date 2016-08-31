using System;

namespace DragonSpark.Composition
{
	public struct ActivationParameter
	{
		public ActivationParameter( IServiceProvider provider, Type sourceType )
		{
			Services = provider;
			SourceType = sourceType;
		}

		public IServiceProvider Services { get; }
		public Type SourceType { get; }
	}
}