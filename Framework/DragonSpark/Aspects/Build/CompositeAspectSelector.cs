using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Build
{
	public class CompositeAspectSelector : ParameterizedItemSourceBase<ITypeDefinition, IAspects>
	{
		readonly ImmutableArray<Func<ITypeDefinition, IEnumerable<IAspects>>> sources;

		public CompositeAspectSelector( params Func<ITypeDefinition, IEnumerable<IAspects>>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<IAspects> Yield( ITypeDefinition parameter )
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