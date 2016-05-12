using DragonSpark.ComponentModel;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Setup.Commands
{
	public class RunProgramCommand : CommandBase<object>
	{
		[Locate]
		public IProgram Program { [return: Required]get; set; }

		public override void Execute( object parameter ) => Program.Run( parameter );
	}
}