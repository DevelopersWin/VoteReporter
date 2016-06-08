using DragonSpark.Diagnostics.Logger;
using DragonSpark.Setup.Registration;
using Serilog;
using System;

namespace DragonSpark.Diagnostics
{
	/*public class TryContextElevated : TryContext
	{
		public TryContextElevated( ILogger logger ) : base( logger.Debug ) {}
	}*/

	[Persistent]
	public struct TryContext
	{
		readonly LogException log;

		public TryContext( ILogger logger ) : this( logger.Verbose ) {}

		public TryContext( LogException log )
		{
			this.log = log;
		}

		public Exception Invoke( Action action )
		{
			try
			{
				action();
			}
			catch ( Exception exception )
			{
				log( exception, "An exception has occurred while executing an application delegate." );
				return exception;
			}
			return null;
		}

		public Result Invoke( Func<object> resolve )
		{
			try
			{
				return new Result( resolve() );
			}
			catch ( Exception e )
			{
				log( e, "An exception has occurred while attempting to create an object via a factory delegate." );
				return new Result( e );
			}
		}

		public struct Result
		{
			public Result( Exception exception ) : this( null, exception ) {}

			public Result( object instance ) : this( instance, null ) {}

			Result( object instance, Exception exception )
			{
				Instance = instance;
				Exception = exception;
			}

			public object Instance { get; }
			public Exception Exception { get; }
		}
	}
}