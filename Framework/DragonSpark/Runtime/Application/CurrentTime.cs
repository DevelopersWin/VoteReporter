using System;

namespace DragonSpark.Runtime.Application
{
	public sealed class CurrentTime : ICurrentTime
	{
		public static CurrentTime Default { get; } = new CurrentTime();
		CurrentTime() {}
		
		public DateTimeOffset Now => DateTimeOffset.Now;
	}
}