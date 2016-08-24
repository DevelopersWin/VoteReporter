using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Testing.Framework.Runtime
{
	public sealed class PurgingContext : InitializedDisposableAction
	{
		readonly static Action Run = PurgeLoggerMessageHistoryCommand.Default.Fixed( Output.Default.Get ).Run;

		public PurgingContext() : base( Run ) {}
	}
}