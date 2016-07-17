using DragonSpark.Configuration;
using System;

namespace DragonSpark.Runtime
{
	public sealed class CurrentTime : ICurrentTime
	{
		public static CurrentTime Default { get; } = new CurrentTime();
		CurrentTime() {}
		
		public DateTimeOffset Now => DateTimeOffset.Now;
	}

	public sealed class CurrentTimeConfiguration : Configuration<ICurrentTime>
	{
		public static CurrentTimeConfiguration Instance { get; } = new CurrentTimeConfiguration();
		public CurrentTimeConfiguration() : base( () => CurrentTime.Default ) {}
	}
}