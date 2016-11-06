namespace DragonSpark.Sources.Parameterized.Caching
{
	public sealed class AmbientContext<T> : DelegatedSource<T>
	{
		public static AmbientContext<T> Default { get; } = new AmbientContext<T>();
		AmbientContext() : base( AmbientStack.GetCurrentItem<T> ) {}
	}
}
