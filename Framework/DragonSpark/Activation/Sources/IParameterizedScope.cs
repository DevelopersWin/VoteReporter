namespace DragonSpark.Activation.Sources
{
	public interface IParameterizedScope<T> : IParameterizedScope<object, T>, IParameterizedSource<T> {}
}