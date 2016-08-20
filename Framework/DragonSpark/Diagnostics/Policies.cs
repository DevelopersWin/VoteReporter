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

	public sealed class RetryPolicyFactory<T> : SourceBase<Policy> where T : Exception
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
		public LogRetryException( ILogger logger ) : base( logger.Information, "Exception encountered during a retry-aware context.  Waiting {Wait} until next attempt..." ) {}
	}

	public abstract class LogExceptionCommandBase<T> : CommandBase<ExceptionParameter<T>>
	{
		readonly Action<Exception, string, object[]> action;
		readonly string messageTemplate;

		protected LogExceptionCommandBase( Action<Exception, string, object[]> action, string messageTemplate )
		{
			this.action = action;
			this.messageTemplate = messageTemplate;
		}

		public override void Execute( ExceptionParameter<T> parameter ) => action( parameter.Exception, messageTemplate, new object[] { parameter.Argument } );

		public void Execute( Exception exception, T argument ) => Execute( new ExceptionParameter<T>( exception, argument ) );
	}

	public struct ExceptionParameter<T>
	{
		public ExceptionParameter( Exception exception, T argument )
		{
			Exception = exception;
			Argument = argument;
		}

		public Exception Exception { get; }
		public T Argument { get; }
	}

	[ApplyAutoValidation]
	public class ApplyPolicyCommand : CommandBase<Action>
	{
		readonly Policy policy;

		public ApplyPolicyCommand( Policy policy  )
		{
			this.policy = policy;
		}

		public override void Execute( Action parameter ) => policy.Execute( parameter );
	}
}
