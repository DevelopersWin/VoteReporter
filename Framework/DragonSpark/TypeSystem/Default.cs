namespace DragonSpark.TypeSystem
{
	static class Default<T>
	{
		public static T Value { get; } = (T)DefaultValues.Default.Get( typeof(T) );
	}
}