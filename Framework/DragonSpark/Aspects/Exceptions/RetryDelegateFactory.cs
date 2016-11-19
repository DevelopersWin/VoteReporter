using DragonSpark.Diagnostics;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Exceptions
{
	public sealed class RetryDelegateFactory : SourceBase<Action<Exception, TimeSpan>>
	{
		public static RetryDelegateFactory Default { get; } = new RetryDelegateFactory();
		RetryDelegateFactory() : this( DragonSpark.Diagnostics.Defaults.Logger.Into( LogRetryException.Default.Get ).Get ) {}

		readonly Func<LogExceptionCommandBase<TimeSpan>> commandSource;

		[UsedImplicitly]
		public RetryDelegateFactory( Func<LogExceptionCommandBase<TimeSpan>> commandSource )
		{
			this.commandSource = commandSource;
		}

		public override Action<Exception, TimeSpan> Get() => commandSource().Execute;
	}
}