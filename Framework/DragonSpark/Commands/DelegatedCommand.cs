using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Commands
{
	public class DelegatedCommand<T> : CommandBase<T>
	{
		readonly Action<T> command;

		public DelegatedCommand( Action<T> command ) 
		{
			this.command = command;
		}

		public override void Execute( [Optional]T parameter ) => command( parameter );
	}
}