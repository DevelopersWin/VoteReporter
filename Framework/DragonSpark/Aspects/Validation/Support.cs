using DragonSpark.Aspects.Build;
using DragonSpark.TypeSystem;
using System.Linq;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Support : DefinitionBase
	{
		public static Support Default { get; } = new Support();
		Support() : this( ParameterizedSourceTypeDefinition.Default, RunTypeDefinition.Default, GenericCommandTypeDefinition.Default, CommandTypeDefinition.Default ) {}

		public Support( params IValidatedTypeDefinition[] definitions ) : base( SpecificationFactory.Default.Get( definitions.SelectTypes() ), definitions.SelectMany( AspectInstanceLocatorFactory.Default.Get ).ToArray() ) {}
	}
}