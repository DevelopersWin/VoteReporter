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
			new Dictionary<ITypeDefinition, IAspectSelector>
			{
				{ GenericCommandTypeDefinition.Default, new TypeAspectSelector<ApplyCommandRelay>( CommandTypeDefinition.Default.ReferencedType ) },
				{ ParameterizedSourceTypeDefinition.Default, new TypeAspectSelector<ApplyParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType ) },
				{ GenericSpecificationTypeDefinition.Default, new TypeAspectSelector<ApplySpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.ReferencedType ) },
			}.ToImmutableDictionary() ) {}
	}
}