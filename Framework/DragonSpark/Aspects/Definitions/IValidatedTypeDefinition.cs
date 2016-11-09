using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Definitions
{
	public interface IValidatedTypeDefinition : ITypeDefinition
	{
		IMethods Validation { get; }
		IMethods Execution { get; }
	}
}