using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using System;
using System.Diagnostics;

namespace DragonSpark.Diagnostics
{
	public class DebugOutputCommand : DelegatedTextCommand
	{
		public static DebugOutputCommand Instance { get; } = new DebugOutputCommand();

		DebugOutputCommand() : base( s => Debug.WriteLine( s ) ) {}
	}

	public class IgnoredOutputCommand : DelegatedTextCommand
	{
		public static IgnoredOutputCommand Instance { get; } = new IgnoredOutputCommand();

		IgnoredOutputCommand() : base( s => {} ) {}
	}

	// [AutoValidation( false )]
	public class DelegatedTextCommand : DelegatedCommand<string>
	{
		public DelegatedTextCommand( Action<string> action ) : base( action, Specifications.Always ) {}

		// public DelegatedTextCommand( Action<string> action, ISpecification<string> specification ) : base( action, specification ) {}
	}
}