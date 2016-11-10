using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Linq;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : this( ParameterizedSourceTypeDefinition.Default, RunCommandTypeDefinition.Default, GenericCommandTypeDefinition.Default, CommandTypeDefinition.Default ) {}
		Definition( params IValidatedTypeDefinition[] definitions ) : base( /*definitions.SelectTypes(),*/ definitions.SelectMany( AspectSelectorFactory.Default.Yield ).ToArray() ) {}
	}

	/*public interface ITypeDefinition : IValidatedTypeDefinition
	{
		Type AdapterType { get; }
	}*/
}