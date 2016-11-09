namespace DragonSpark.Aspects.Adapters
{
	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>
	{
		protected abstract void Execute( T parameter );

		public sealed override object Invoke( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}
}