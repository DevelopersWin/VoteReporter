using System.Runtime.InteropServices;
using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
	public class FixedCommand<T> : DisposingCommand<object>
	{
		readonly ICommand<T> command;
		readonly T parameter;

		public FixedCommand( ICommand<T> command, [Optional]T parameter ) : this( command, Specifications.Specifications.Always, parameter ) {}

		public FixedCommand( ICommand<T> command, ISpecification<object> specification, [Optional]T parameter ) : base( specification )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override void Execute( [Optional]object _ ) => command.Execute( parameter );

		protected override void OnDispose() => command.TryDispose();
	}
}