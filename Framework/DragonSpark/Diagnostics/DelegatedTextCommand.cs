using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Diagnostics
{
	public class DelegatedTextCommand : DelegatedCommand<string>
	{
		public DelegatedTextCommand( Action<string> action ) : base( action, Specifications.Always ) {}
	}
}