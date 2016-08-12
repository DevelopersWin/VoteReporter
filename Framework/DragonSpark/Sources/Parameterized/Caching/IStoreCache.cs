namespace DragonSpark.Sources.Parameterized.Caching
{
	public interface IStoreCache<in TInstance, TValue> : ICache<TInstance, IAssignableSource<TValue>> {}
}