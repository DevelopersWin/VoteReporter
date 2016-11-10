using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	sealed class Definitions : AspectBuildDefinition
	{
		public static Definitions Default { get; } = new Definitions();
		Definitions() : this( ApplyCommandRelayDefinition.Default, ApplySourceRelayDefinition.Default, ApplySpecificationRelayDefinition.Default ) {}
		Definitions( params IAspectBuildDefinition[] definitions ) : base( 
			definitions.Concat().Prepend( 
				new TypeAspectSource<ApplyCommandRelay>( CommandTypeDefinition.Default.ReferencedType ),
				new TypeAspectSource<ApplyParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ),
				new TypeAspectSource<ApplySpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.ReferencedType )
			).Fixed()
		) {}
	}
}