using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Threading.Tasks;

namespace DragonSpark.Tasks
{
	public class FixedTaskSource<T> : FixedTaskSource
	{
		public FixedTaskSource( ICommand<T> command, T parameter ) : base( command.Fixed( parameter ).ToRunDelegate() ) {}
		public FixedTaskSource( ICommand<T> command, Func<T> parameter ) : base( command.Fixed( parameter() ).ToRunDelegate() ) {}
	}

	public class FixedTaskSource : FixedFactory<Action, Task>
	{
		readonly static Func<Action, Task> Run = Task.Run;
		
		public FixedTaskSource( Action action ) : base( Run, action ) {}
	}
}