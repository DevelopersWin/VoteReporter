using System;
using System.Collections.Generic;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectDefinitionSelector : ParameterizedItemSourceBase<ITypeDefinition, IAspectDefinition>
	{
		readonly Func<ITypeDefinition, IEnumerable<IAspectDefinition>> selectors;

		public AspectDefinitionSelector( Func<ITypeDefinition, IEnumerable<IAspectDefinition>> selectors )
		{
			this.selectors = selectors;
		}

		public override IEnumerable<IAspectDefinition> Yield( ITypeDefinition parameter ) => selectors( parameter );
	}
}