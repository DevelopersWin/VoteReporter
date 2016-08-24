using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;

namespace DragonSpark.Testing.Framework.Runtime
{
	public sealed class PurgingContext : InitializedDisposableAction
	{
		public PurgingContext() : base( PurgeLoggerMessageHistoryCommand.Default.Fixed( Output.Default.Get() ).Run ) {}
	}
}