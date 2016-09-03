namespace DragonSpark.Commands
{
	public abstract class RunCommandBase : DisposingCommand<object>, IRunCommand
	{
		public sealed override void Execute( object _ ) => Execute();

		public abstract void Execute();
	}
}