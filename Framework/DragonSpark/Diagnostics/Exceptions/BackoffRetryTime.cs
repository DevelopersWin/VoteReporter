using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class BackoffRetryTime : RetryTimeBase
	{
		public static BackoffRetryTime Default { get; } = new BackoffRetryTime();
		BackoffRetryTime() : base( parameter => (int)Math.Pow( parameter, 2 ) ) {}
	}
}