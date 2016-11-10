using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	/*public interface IAdapter<in TParameter, out TResult> : IParameterizedSource<TParameter, TResult>, IAdapter {}*/

	public interface IAdapter : IParameterizedSource<object, object> {}
}