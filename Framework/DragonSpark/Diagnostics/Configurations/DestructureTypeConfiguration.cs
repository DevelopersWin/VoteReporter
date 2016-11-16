using PostSharp.Patterns.Contracts;
using Serilog.Configuration;
using System;

namespace DragonSpark.Diagnostics.Configurations
{
	public class DestructureTypeConfiguration : DestructureConfigurationBase
	{
		[Required]
		public Type ScalarType { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.AsScalar( ScalarType );
	}
}