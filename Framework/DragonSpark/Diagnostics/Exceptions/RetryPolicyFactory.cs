using DragonSpark.Sources.Parameterized;
using Polly;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class RetryPolicyFactory : ParameterizedSourceBase<RetryPolicyParameter, Policy>
	{
		public static RetryPolicyFactory Default { get; } = new RetryPolicyFactory();
		RetryPolicyFactory() {}

		public override Policy Get( RetryPolicyParameter parameter ) => parameter.Source().WaitAndRetry( parameter.NumberOfRetries, parameter.Time, parameter.OnRetry );
	}
}