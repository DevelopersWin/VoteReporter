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

		public static void Retry<T>( Action action ) where T : Exception => Apply( RetryPolicyFactory<T>.Instance.Get, action );

		public static void Apply( Func<Policy> policy, Action action ) => Commands.Get( policy() ).Execute( action );

		public static ICommand<Action> ToCommand( this ISource<Policy> @this ) => Commands.Get( @this.Get() );
	}

	public class RetryPolicyFactory<T> : SourceBase<Policy> where T : Exception
	{
		public static ISource<Policy> Instance { get; } = new Scope<Policy>( Factory.Global( () => new RetryPolicyFactory<T>( new LogRetryException( Logger.Instance.ToScope().Get() ).Execute ).Get() ) );
		
		readonly Action<Exception, TimeSpan> onRetry;

		public RetryPolicyFactory( Action<Exception, TimeSpan> onRetry )
		{
			this.onRetry = onRetry;
		}

		public override Policy Get() => Policy.Handle<T>().WaitAndRetry( 5, i => TimeSpan.FromSeconds( Math.Pow( i, 2 ) ), onRetry );
	}

	public sealed class LogRetryException : LogExceptionCommandBase<TimeSpan>
	{
		public LogRetryException( ILogger logger ) : this( logger.Information ) {}

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
