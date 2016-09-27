﻿using DragonSpark.Diagnostics.Exceptions;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using Serilog;
using System;

namespace DragonSpark.Aspects.Exceptions
{
	public static class Time
	{
		public static Func<int, TimeSpan> None { get; } = i => TimeSpan.Zero;

		public static Func<int, TimeSpan> Default { get; } = LinearRetryTime.Default.ToSourceDelegate();
	}

	class RetryDelegateFactory : SourceBase<Action<Exception, TimeSpan>>
	{
		public static RetryDelegateFactory Default { get; } = new RetryDelegateFactory();
		RetryDelegateFactory() : this( Logger.Default.ToScope().Get, LogRetryException.Defaults.Get ) {}

		readonly Func<ILogger> loggerSource;
		readonly Func<ILogger, LogExceptionCommandBase<TimeSpan>> commandSource;

		public RetryDelegateFactory( Func<ILogger> loggerSource, Func<ILogger, LogExceptionCommandBase<TimeSpan>> commandSource )
		{
			this.loggerSource = loggerSource;
			this.commandSource = commandSource;
		}

		public override Action<Exception, TimeSpan> Get() => commandSource( loggerSource() ).Execute;
	}
}