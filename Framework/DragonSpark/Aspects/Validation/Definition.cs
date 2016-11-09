using DragonSpark.Aspects.Build;
using System.Linq;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : this( ParameterizedSourceTypeDefinition.Default, RunTypeDefinition.Default, GenericCommandTypeDefinition.Default, CommandTypeDefinition.Default ) {}
		Definition( params IValidatedTypeDefinition[] definitions ) : base( /*definitions.SelectTypes(),*/ definitions.SelectMany( AspectInstanceLocatorFactory.Default.Get ).ToArray() ) {}
	}
}