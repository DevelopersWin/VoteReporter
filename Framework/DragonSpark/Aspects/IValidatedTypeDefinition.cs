using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects
{
	public interface IValidatedTypeDefinition : ITypeDefinition
	{
		IMethods Validation { get; }
		IMethods Execution { get; }
	}
}