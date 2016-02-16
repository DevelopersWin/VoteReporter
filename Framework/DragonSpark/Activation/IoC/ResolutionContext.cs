using DragonSpark.Properties;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;

namespace DragonSpark.Activation.IoC
{
	class ResolutionContext
	{
		readonly ILogger messageLogger;

		public ResolutionContext( [Required]ILogger messageLogger )
		{
			this.messageLogger = messageLogger;
		}

		public object Execute( Func<object> resolve )
		{
			try
			{
				var result = resolve();
				return result;
			}
			catch ( ResolutionFailedException e )
			{
				messageLogger.Debug( e, string.Format( Resources.Activator_CouldNotActivate, e.TypeRequested, e.NameRequested ?? Resources.Activator_None ) );
				return null;
			}
		}
	}
}