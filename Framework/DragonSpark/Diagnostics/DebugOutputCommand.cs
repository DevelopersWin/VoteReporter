using System;
using System.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;

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

	public class DelegatedTextCommand : DelegatedCommand<string>
	{
		public DelegatedTextCommand( Action<string> action ) : this( action, Specification<string>.Instance ) {}

		public DelegatedTextCommand( Action<string> action, ISpecification<string> specification ) : base( action, specification ) {}
	}
}