using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Sources;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Destructurers;
using System;
using System.Collections.Generic;
using System.Composition;

namespace DragonSpark.Windows.Diagnostics
{
	public sealed class ApplyExceptionDetails : LoggingConfigurationBase
	{
		readonly Func<IEnumerable<IExceptionDestructurer>> source;

		[Export( typeof(ILoggingConfiguration) )]
		public static ApplyExceptionDetails Default { get; } = new ApplyExceptionDetails();
		ApplyExceptionDetails() : this( ExceptionEnricher.DefaultDestructurers.IncludeExports ) {}

		ApplyExceptionDetails( Func<IEnumerable<IExceptionDestructurer>> source )
		{
			this.source = source;
		}

		public override LoggerConfiguration Get( LoggerConfiguration parameter ) => 
			parameter.Enrich.WithExceptionDetails( source() );
	}
}