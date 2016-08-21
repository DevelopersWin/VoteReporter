using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using Polly;
using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	public static class Policies
	{
		readonly static IParameterizedSource<Policy, ICommand<Action>> Commands = new DecoratedCache<Policy, ApplyPolicyCommand>();

		public static void Retry<T>( Action action ) where T : Exception => Apply( Defaults<T>.Retry.ToDelegate(), action );

		public static void Apply( Func<Policy> policy, Action action ) => Commands.Get( policy() ).Execute( action );

		public static ICommand<Action> ToCommand( this ISource<Policy> @this ) => Commands.Get( @this.Get() );
	}

	public static class Defaults<T> where T : Exception
	{
		public static ISource<Policy> Retry { get; } = new Scope<Policy>( new FixedFactory<RetryPolicyParameter, Policy>( RetryPolicyFactory.Instance.Get, RetryPolicyParameterSource<T>.Instance.Get ).Global() );
	}

	public sealed class RetryPolicyParameterSource<T> : SourceBase<RetryPolicyParameter> where T : Exception
	{
		public static RetryPolicyParameterSource<T> Instance { get; } = new RetryPolicyParameterSource<T>();
		RetryPolicyParameterSource() {}

		public override RetryPolicyParameter Get() => new RetryPolicyParameter( PolicyBuilderSource<T>.Instance.Get, Defaults.Time, LogRetryException.Defaults.Get( Logger.Instance.ToScope().Get() ).Execute );
	}

	public static class Defaults
	{
		public static Func<int, TimeSpan> Time { get; } = LinearRetryTime.Instance.ToSourceDelegate();
	}

	public sealed class RetryPolicyFactory : ParameterizedSourceBase<RetryPolicyParameter, Policy>
	{
		public static RetryPolicyFactory Instance { get; } = new RetryPolicyFactory();
		RetryPolicyFactory() {}

		public override Policy Get( RetryPolicyParameter parameter ) => parameter.Source().WaitAndRetry( parameter.NumberOfRetries, parameter.Time, parameter.OnRetry );
	}

	public class PolicyBuilderSource<T> : SourceBase<PolicyBuilder> where T : Exception
	{
		public static PolicyBuilderSource<T> Instance { get; } = new PolicyBuilderSource<T>();
		protected PolicyBuilderSource() {}

		public override PolicyBuilder Get() => Policy.Handle<T>();
	}

	public struct RetryPolicyParameter
	{
		// public RetryPolicyParameter( Func<PolicyBuilder> source, int numberOfRetries = 5 ) : this( source, Defaults.Time, Defaults.Retry.Get(), numberOfRetries ) {}

		public RetryPolicyParameter( Func<PolicyBuilder> source, Func<int, TimeSpan> time, Action<Exception, TimeSpan> onRetry, int numberOfRetries = 5 )
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

	public sealed class BackoffRetryTime : RetryTimeBase
	{
		public static BackoffRetryTime Instance { get; } = new BackoffRetryTime();
		BackoffRetryTime() : base( parameter => (int)Math.Pow( parameter, 2 ) ) {}
	}

	public sealed class LinearRetryTime : RetryTimeBase
	{
		public static LinearRetryTime Instance { get; } = new LinearRetryTime();
		LinearRetryTime() : base( parameter => parameter ) {}
	}

	public abstract class RetryTimeBase : ParameterizedSourceBase<int, TimeSpan>
	{
		readonly Transform<int> time;

		protected RetryTimeBase( Transform<int> time )
		{
			this.time = time;
		}

		public override TimeSpan Get( int parameter ) => TimeSpan.FromSeconds( time( parameter ) );
	}

	public sealed class LogRetryException : LogExceptionCommandBase<TimeSpan>
	{
		public static IParameterizedSource<ILogger, LogRetryException> Defaults { get; } = new Cache<ILogger, LogRetryException>( logger => new LogRetryException( logger ) );
		LogRetryException( ILogger logger ) : this( logger.Information ) {}

		public LogRetryException( LogException<TimeSpan> action ) : base( action, "Exception encountered during a retry-aware context.  Waiting {Wait} until next attempt..." ) {}
	}

	[ApplyAutoValidation]
	public sealed class ApplyPolicyCommand : CommandBase<Action>
	{
		readonly Policy policy;

		public ApplyPolicyCommand( Policy policy )
		{
			this.policy = policy;
		}

		public override void Execute( Action parameter ) => policy.Execute( parameter );
	}
}
