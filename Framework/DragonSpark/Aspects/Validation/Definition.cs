using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Coercion;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base(
			AspectSelection.Default.Allow( ValidatedCastCoercer<ITypeDefinition, IValidatedTypeDefinition>.Default ),
			
			ParameterizedSourceTypeDefinition.Default, 
			RunCommandTypeDefinition.Default, 
			GenericCommandTypeDefinition.Default, 
			CommandTypeDefinition.Default 
		) {}
	}
}