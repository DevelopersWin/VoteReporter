using DragonSpark.Application;
using DragonSpark.Aspects.Extensibility;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;
using System.Composition;

namespace DragonSpark.Composition
{
	// [ApplyAutoValidation]
	public sealed class InitializeExportsCommand : ExtensibleCommandBase<IServiceProvider>
	{
		public static InitializeExportsCommand Default { get; } = new InitializeExportsCommand();
		InitializeExportsCommand()  {}

		public override void Execute( IServiceProvider parameter ) => Exports.Default.Assign( new ExportProvider( parameter.Get<CompositionContext>() ) );
	}
}