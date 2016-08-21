namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ActivatedCache<T> : ActivatedCache<object, T>, ICache<T> where T : class, new()
	{
		public new static ActivatedCache<T> Default { get; } = new ActivatedCache<T>();
		public ActivatedCache() {}
	}
}