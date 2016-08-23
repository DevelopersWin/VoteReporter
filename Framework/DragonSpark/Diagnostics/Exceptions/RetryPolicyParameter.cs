using System;
using Polly;

namespace DragonSpark.Diagnostics.Exceptions
{
	public struct RetryPolicyParameter
	{
		public RetryPolicyParameter( Func<PolicyBuilder> source, Func<int, TimeSpan> time, Action<Exception, TimeSpan> onRetry, int numberOfRetries = Defaults.Retries )
		{
			Source = source;
			Time = time;
			OnRetry = onRetry;
			NumberOfRetries = numberOfRetries;
		}

		public Func<PolicyBuilder> Source { get; }
		public Func<int, TimeSpan> Time { get; }
		public Action<Exception, TimeSpan> OnRetry { get; }
		public int NumberOfRetries { get; }
	}
}