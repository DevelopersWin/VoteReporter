using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects
{
	public interface IValidatedComponentDefinition : IDefinition
	{
		IMethodStore Validation { get; }
		IMethodStore Execution { get; }
	}
}