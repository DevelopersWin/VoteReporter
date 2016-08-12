using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.Sources
{
	public class StackCommand<T> : DisposingCommand<T>
	{
		public StackCommand( [Required]IStack<T> stack )
		{
			Stack = stack;
		}

		protected IStack<T> Stack { get; }

		public override void Execute( T parameter ) => Stack.Push( parameter );

		protected override void OnDispose() => Stack.Pop().TryDispose();
	}
}