using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectDefinitionSelector : ParameterizedItemSourceBase<ITypeDefinition, IAspects>
	{
		readonly Func<ITypeDefinition, IEnumerable<IAspects>> selectors;

		public AspectDefinitionSelector( Func<ITypeDefinition, IEnumerable<IAspects>> selectors )
		{
			this.selectors = selectors;
		}

		public override IEnumerable<IAspects> Yield( ITypeDefinition parameter ) => selectors( parameter );
	}
}