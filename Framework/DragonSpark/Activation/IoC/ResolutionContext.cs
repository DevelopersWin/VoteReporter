using DragonSpark.Properties;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	[Persistent]
	class ResolutionContext
	{
		readonly ILogger logger;

		// public ResolutionContext( LoggerSource logger ) : this( logger.Create ) {}

		public ResolutionContext( ILogger logger )
		{
			this.logger = logger;
		}

		public object Execute( Func<object> resolve )
		{
			try
			{
				return resolve();
			}
			catch ( ResolutionFailedException e )
			{
				logger.Debug( e, string.Format( Resources.Activator_CouldNotActivate, e.TypeRequested, e.NameRequested ?? Resources.Activator_None ) );
				return null;
			}
		}
	}
}