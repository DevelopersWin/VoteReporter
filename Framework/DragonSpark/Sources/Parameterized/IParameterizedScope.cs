namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedScope<T> : IParameterizedScope<object, T>, IParameterizedSource<T> {}
}