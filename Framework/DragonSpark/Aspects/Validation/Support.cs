using System.Linq;
using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Support : SupportDefinitionBase
	{
		public static Support Default { get; } = new Support();
		Support() : this( ParameterizedSourceDefinition.Default, GenericCommandDefinition.Default, CommandDefinition.Default ) {}

		public Support( params IValidatedComponentDefinition[] definitions ) : base( SpecificationFactory.Default.Get( definitions ), definitions.SelectMany( AspectInstanceLocatorFactory.Default.Get ).ToArray() ) {}
	}
}