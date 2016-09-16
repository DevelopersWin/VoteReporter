using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public sealed class AspectInstances : ParameterizedSourceBase<Type, IEnumerable<AspectInstance>>
	{
		readonly ImmutableArray<IAspectSource> sources;

		public AspectInstances( IEnumerable<IProfile> sources ) : this( sources.Concat() ) {}

		public AspectInstances( IEnumerable<IAspectSource> sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var source in sources )
			{
				var instance = source.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}
}