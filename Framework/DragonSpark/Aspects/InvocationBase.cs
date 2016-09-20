namespace DragonSpark.Aspects
{
	public abstract class InvocationBase<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		public abstract TResult Invoke( TParameter parameter );

		object IInvocation.Invoke( object parameter ) => Invoke( (TParameter)parameter );
	}

	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>, IInvocation<T>
	{
		protected abstract void Execute( T parameter );

		public sealed override object Invoke( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}
}