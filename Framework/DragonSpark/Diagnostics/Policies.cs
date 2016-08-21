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

		static void Apply( Func<Policy> policy, Action action ) => Commands.Get( policy() ).Execute( action );

		public static ICommand<T> Apply<T>( this ICommand<T> @this, ISource<Policy> source ) => Apply( @this, source.ToDelegate() );
		public static ICommand<T> Apply<T>( this ICommand<T> @this, Func<Policy> source ) => new PolicyDecoratedCommand<T>( source, @this );
	}

	public static class Defaults
	{
		public const int Retries = 5;

		public static Func<int, TimeSpan> Time { get; } = LinearRetryTime.Default.ToSourceDelegate();
	}

	public static class Retry
	{
		public static ISource<Policy> Create<T>( int retries = Defaults.Retries ) where T : Exception => Create( RetryPolicyParameterSource<T>.Default.Fixed( retries ) );

		public static ISource<Policy> Create( ISource<RetryPolicyParameter> source ) => 
			new FixedFactory<RetryPolicyParameter, Policy>( RetryPolicyFactory.Default.Get, source.Get );
	}

	public static class Defaults<T> where T : Exception
	{
		public static ISource<Policy> Retry { get; } = new Scope<Policy>( Diagnostics.Retry.Create<T>().Global() );
	}

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

	public sealed class RetryPolicyFactory : ParameterizedSourceBase<RetryPolicyParameter, Policy>
	{
		public static RetryPolicyFactory Default { get; } = new RetryPolicyFactory();
		RetryPolicyFactory() {}

		public override Policy Get( RetryPolicyParameter parameter ) => parameter.Source().WaitAndRetry( parameter.NumberOfRetries, parameter.Time, parameter.OnRetry );
	}

	public class PolicyBuilderSource<T> : SourceBase<PolicyBuilder> where T : Exception
	{
		public static PolicyBuilderSource<T> Default { get; } = new PolicyBuilderSource<T>();
		protected PolicyBuilderSource() {}

		public override PolicyBuilder Get() => Policy.Handle<T>();
	}

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

	public sealed class BackoffRetryTime : RetryTimeBase
	{
		public static BackoffRetryTime Default { get; } = new BackoffRetryTime();
		BackoffRetryTime() : base( parameter => (int)Math.Pow( parameter, 2 ) ) {}
	}

	public sealed class LinearRetryTime : RetryTimeBase
	{
		public static LinearRetryTime Default { get; } = new LinearRetryTime();
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

	public sealed class PolicyDecoratedCommand<T> : DecoratedCommand<T>
	{
		readonly Func<Policy> source;

		public PolicyDecoratedCommand( Func<Policy> source, ICommand<T> inner ) : base( inner )
		{
			this.source = source;
		}

		public override void Execute( T parameter ) => source().Execute( () => base.Execute( parameter ) );
	}
}
