namespace DragonSpark.Sources.Caching
{
	public interface IStoreCache<in TInstance, TValue> : ICache<TInstance, IAssignableSource<TValue>> {}
}