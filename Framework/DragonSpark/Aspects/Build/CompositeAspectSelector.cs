using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Build
{
	public class CompositeAspectSelector : ParameterizedItemSourceBase<ITypeDefinition, IAspectDefinition>
	{
		readonly ImmutableArray<Func<ITypeDefinition, IEnumerable<IAspectDefinition>>> sources;

		public CompositeAspectSelector( params Func<ITypeDefinition, IEnumerable<IAspectDefinition>>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<IAspectDefinition> Yield( ITypeDefinition parameter )
		{
			foreach ( var source in sources )
			{
				foreach ( var aspectDefinition in source( parameter ) )
				{
					yield return aspectDefinition;
				}
			}
		}
	}
}