namespace DragonSpark.Sources.Parameterized
{
	public interface ITransformer<T> : IParameterizedSource<T, T> {}
	public delegate T Transform<T>( T parameter );
}