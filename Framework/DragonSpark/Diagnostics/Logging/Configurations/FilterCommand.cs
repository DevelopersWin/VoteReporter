using System.Windows.Markup;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog.Configuration;
using Serilog.Core;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	[ContentProperty( nameof(Items) )]
	public class FilterCommand : FilterCommandBase
	{
		public DeclarativeCollection<ILogEventFilter> Items { get; } = new DeclarativeCollection<ILogEventFilter>();

		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.With( Items.Fixed() );
	}
}