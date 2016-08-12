namespace DragonSpark.Activation.Sources
{
	public interface IAssignableParameterizedSource<T> : IAssignableParameterizedSource<object, T>, IParameterizedSource<T> {}
}