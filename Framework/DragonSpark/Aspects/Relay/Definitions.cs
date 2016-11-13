using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Extensions;

namespace DragonSpark.Aspects.Relay
{
	sealed class Definitions : MappedAspectBuildDefinition
	{
		public static Definitions Default { get; } = new Definitions();
		Definitions() : base( 
			new Dictionary<ITypeDefinition, IEnumerable<IAspects>>
			{
				{ GenericCommandTypeDefinition.Default, new TypeAspects<ApplyCommandRelay>( CommandTypeDefinition.Default ).Append<IAspects>( new TypeAspects<ApplySpecificationRelay>( CommandTypeDefinition.Default ) ) },
				{ ParameterizedSourceTypeDefinition.Default, new TypeAspects<ApplyParameterizedSourceRelay>( GeneralizedParameterizedSourceTypeDefinition.Default ).Yield() },
				{ SpecificationTypeDefinition.Default, new TypeAspects<ApplySpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default ).Yield() },
			}.ToImmutableDictionary() ) {}
	}
}