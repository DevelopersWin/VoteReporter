using DragonSpark.Aspects.Build;
using DragonSpark.TypeSystem;
using System.Linq;

namespace DragonSpark.Aspects.Validation
{
	public sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : this( ParameterizedSourceTypeDefinition.Default, RunTypeDefinition.Default, GenericCommandTypeDefinition.Default, CommandTypeDefinition.Default ) {}
		Support( params IValidatedTypeDefinition[] definitions ) : base( definitions.SelectTypes(), definitions.SelectMany( AspectInstanceLocatorFactory.Default.Get ).ToArray() ) {}
	}
}