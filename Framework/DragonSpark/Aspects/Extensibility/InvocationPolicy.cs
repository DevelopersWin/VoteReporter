namespace DragonSpark.Aspects.Extensibility
{
	public interface IInvocation<in TParameter, out TResult> : IInvocation
	{
		TResult Invoke( TParameter parameter );
	}

	public interface IInvocation
	{
		object Invoke( object parameter );
	}
}
