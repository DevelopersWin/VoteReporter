using System;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Polly;

namespace DragonSpark.Diagnostics.Exceptions
{
	public sealed class RetryPolicyParameterSource<T> : ParameterizedSourceBase<int, RetryPolicyParameter> where T : Exception
	{
		readonly static Func<PolicyBuilder> Source = PolicyBuilderSource<T>.Default.Get;
		public static RetryPolicyParameterSource<T> Default { get; } = new RetryPolicyParameterSource<T>();
		RetryPolicyParameterSource() : this( Defaults.Time ) {}

		readonly Func<int, TimeSpan> time;

		public RetryPolicyParameterSource( Func<int, TimeSpan> time )
		{
			this.time = time;
		}

		public override RetryPolicyParameter Get( int parameter ) => new RetryPolicyParameter( Source, time, LogRetryException.Defaults.Get( Logger.Default.ToScope().Get() ).Execute, parameter );
	}
}