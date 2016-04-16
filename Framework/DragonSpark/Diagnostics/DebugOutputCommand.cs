using System.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Diagnostics
{
	public class DebugOutputCommand : DelegatedCommand<string>
	{
		public static DebugOutputCommand Instance { get; } = new DebugOutputCommand();

		public DebugOutputCommand() : this( Specification<string>.Instance ) {}

		public DebugOutputCommand( ISpecification<string> specification ) : base( s => Debug.WriteLine( s ), specification ) {}
	}
}