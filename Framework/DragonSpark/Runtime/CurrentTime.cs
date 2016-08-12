using System;
using DragonSpark.Activation.Sources;

namespace DragonSpark.Runtime
{
	public sealed class CurrentTime : ICurrentTime
	{
		public static CurrentTime Default { get; } = new CurrentTime();
		CurrentTime() {}
		
		public DateTimeOffset Now => DateTimeOffset.Now;
	}

	public sealed class CurrentTimeConfiguration : Scope<ICurrentTime>
	{
		public static CurrentTimeConfiguration Instance { get; } = new CurrentTimeConfiguration();
		CurrentTimeConfiguration() : base( () => CurrentTime.Default ) {}
	}
}