using System;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Polly;

namespace DragonSpark.Diagnostics.Exceptions
{
	public static class Retry
	{
		public static ISource<Policy> Create<T>( int retries = Defaults.Retries ) where T : Exception => Create( RetryPolicyParameterSource<T>.Default.Fixed( retries ) );

		public static ISource<Policy> Create( ISource<RetryPolicyParameter> source ) => 
			new SuppliedSource<RetryPolicyParameter, Policy>( RetryPolicyFactory.Default.Get, source.Get );
	}
}