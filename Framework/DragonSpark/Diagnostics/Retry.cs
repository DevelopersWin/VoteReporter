using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	[ApplyAutoValidation]
	public class RetryCommand : CommandBase<Action>
	{
		public static RetryCommand Instance { get; } = new RetryCommand();
		RetryCommand() : this( 3  ) {}

		readonly ILogger logger;
		readonly TimeSpan wait;
		readonly int maximumRetryAttempts;

		public RetryCommand( int maximumRetryAttempts ) : this( TimeSpan.FromSeconds( 1 ), maximumRetryAttempts ) {}

		public RetryCommand( TimeSpan wait, int maximumRetryAttempts ) : this( Logger.Instance.ToScope().Get(), wait, maximumRetryAttempts ) {}

		public RetryCommand( ILogger logger, TimeSpan wait, int maximumRetryAttempts )
		{
			this.logger = logger;
			this.wait = wait;
			this.maximumRetryAttempts = maximumRetryAttempts;
		}

		public override void Execute( Action parameter )
		{
			var exceptions = new List<Exception>();

			for ( var retry = 0; retry < maximumRetryAttempts; retry++ )
			{
				try
				{
					if ( retry > 0 )
					{
						System.Threading.Tasks.Task.Delay( wait ).Wait();
					}
					parameter();

					if ( exceptions.Any() )
					{
						logger.Warning( new AggregateException( exceptions ), "{Name} encountered errors while executing a retry block.", GetType().Name );
					}

					return;
				}
				catch ( Exception ex )
				{
					exceptions.Add( ex );
				}
			}

			throw new AggregateException( exceptions );
		}
	}
}
