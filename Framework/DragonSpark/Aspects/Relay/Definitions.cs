using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	sealed class Definitions : PairedAspectBuildDefinition
	{
		public static Definitions Default { get; } = new Definitions();
		Definitions() : base( 
			new Dictionary<ITypeDefinition, IAspectDefinition>
			{
				{ GenericCommandTypeDefinition.Default, new TypeAspectDefinition<ApplyCommandRelay>( CommandTypeDefinition.Default ) },
				{ ParameterizedSourceTypeDefinition.Default, new TypeAspectDefinition<ApplyParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default ) },
				{ GenericSpecificationTypeDefinition.Default, new TypeAspectDefinition<ApplySpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default ) },
			}.ToImmutableDictionary() ) {}
	}
}