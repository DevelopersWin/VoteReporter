using DragonSpark.Aspects.Build;
using System.Linq;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : this( ParameterizedSourceTypeDefinition.Default, RunCommandTypeDefinition.Default, GenericCommandTypeDefinition.Default, CommandTypeDefinition.Default ) {}
		Definition( params IValidatedTypeDefinition[] definitions ) : base( /*definitions.SelectTypes(),*/ definitions.SelectMany( AspectInstanceLocatorFactory.Default.Get ).ToArray() ) {}
	}
}