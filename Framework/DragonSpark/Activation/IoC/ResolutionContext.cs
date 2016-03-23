using DragonSpark.Properties;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	[Persistent]
	class ResolutionContext
	{
		readonly Func<ILogger> logger;

		public ResolutionContext( [Required]Func<ILogger> logger )
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
				logger().Debug( e, string.Format( Resources.Activator_CouldNotActivate, e.TypeRequested, e.NameRequested ?? Resources.Activator_None ) );
				return null;
			}
		}
	}
}