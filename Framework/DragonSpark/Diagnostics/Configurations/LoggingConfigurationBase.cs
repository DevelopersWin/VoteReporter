using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Diagnostics.Configurations
{
	public abstract class LoggingConfigurationBase : AlterationBase<Serilog.LoggerConfiguration>, ILoggingConfiguration {}

	public abstract class LoggingConfigurationBase<T> : LoggingConfigurationBase
	{
		readonly Func<Serilog.LoggerConfiguration, T> @select;

		protected LoggingConfigurationBase( Func<Serilog.LoggerConfiguration, T> @select )
		{
			this.@select = @select;
		}

		protected abstract void Configure( T configuration );

		public override Serilog.LoggerConfiguration Get( Serilog.LoggerConfiguration parameter )
		{
			Configure( @select( parameter ) );
			return parameter;
		}
	}
}