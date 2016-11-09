using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	/*public interface IInvocation<in T> : IInvocation<T, object> {}
	public interface IInvocation<in TParameter, out TResult> : IInvocation
	{
		TResult Invoke( TParameter parameter );
	}*/

	public interface IInvocation : IParameterizedSource<object, object> {}
}