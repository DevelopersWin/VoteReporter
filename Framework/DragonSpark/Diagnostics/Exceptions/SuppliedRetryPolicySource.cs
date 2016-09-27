using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Polly;
using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	public static class Retry
	{
		public static void Execute<T>( Action action ) where T : Exception => SuppliedRetryPolicySource<T>.Default.Get().Execute( action );
	}

	public class SuppliedRetryPolicySource<T> : SuppliedSource<int, Policy> where T : Exception
	{
		const int Retries = 5;

		public static ISource<Policy> Default { get; } = new Scope<Policy>( new SuppliedRetryPolicySource<T>().GlobalCache() );
		SuppliedRetryPolicySource() : this( Retries ) {}

		public SuppliedRetryPolicySource( int parameter ) : this( RetryPolicySource<T>.Default, parameter ) {}
		public SuppliedRetryPolicySource( IParameterizedSource<int, Policy> source, int parameter = Retries ) : base( source.Get, parameter ) {}
	}
}
