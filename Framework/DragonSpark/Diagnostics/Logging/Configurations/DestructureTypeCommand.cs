using System;
using PostSharp.Patterns.Contracts;
using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	public class DestructureTypeCommand : DestructureCommandBase
	{
		[Required]
		public Type ScalarType { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.AsScalar( ScalarType );
	}
}