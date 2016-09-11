namespace DragonSpark.Aspects.Extensibility
{
	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>, IInvocation<T>
	{
		public sealed override object Invoke( T parameter )
		{
			Execute( parameter );
			return null;
		}

		public abstract void Execute( T parameter );
	}
}