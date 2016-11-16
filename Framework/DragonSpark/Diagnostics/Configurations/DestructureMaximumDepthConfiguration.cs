using PostSharp.Patterns.Contracts;
using Serilog.Configuration;

namespace DragonSpark.Diagnostics.Configurations
{
	public class DestructureMaximumDepthConfiguration : DestructureConfigurationBase
	{
		[Range( 0, 100 )]
		public int MaximumDepth { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ToMaximumDepth( MaximumDepth );
	}
}