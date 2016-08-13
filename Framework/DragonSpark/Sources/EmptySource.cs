namespace DragonSpark.Sources
{
	public class EmptySource<T> : Source<T>
	{
		public static EmptySource<T> Instance { get; } = new EmptySource<T>();
		EmptySource() : base( default(T) ) {}
	}
}