using System.Windows.Markup;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog.Configuration;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	[ContentProperty( nameof(Policies) )]
	public class DestructureCommand : DestructureCommandBase
	{
		public DeclarativeCollection<IDestructuringPolicy> Policies { get; } = new DeclarativeCollection<IDestructuringPolicy>();

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.With( Policies.Fixed() );
	}
}