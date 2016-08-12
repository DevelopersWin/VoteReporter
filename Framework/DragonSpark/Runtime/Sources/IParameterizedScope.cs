namespace DragonSpark.Runtime.Sources
{
	public interface IParameterizedScope<T> : IParameterizedScope<object, T>, IParameterizedSource<T> {}
}