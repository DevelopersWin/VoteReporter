using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	sealed class Definitions : MappedAspectBuildDefinition
	{
		public static Definitions Default { get; } = new Definitions();
		Definitions() : base( 
			new Dictionary<ITypeDefinition, IAspects>
			{
				{ GenericCommandTypeDefinition.Default, new TypeAspects<ApplyCommandRelay>( CommandTypeDefinition.Default ) },
				{ ParameterizedSourceTypeDefinition.Default, new TypeAspects<ApplyParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default ) },
				{ SpecificationTypeDefinition.Default, new TypeAspects<ApplySpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default ) },
			}.ToImmutableDictionary() ) {}
	}
}