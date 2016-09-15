using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterSources : ParameterizedSourceBase<Type, IAdapterSource>
	{
		public static IParameterizedSource<Type, IAdapterSource> Default { get; } = new AdapterSources().ToCache();
		AdapterSources() : this( Defaults.Sources ) {}

		readonly ImmutableArray<IAdapterSource> sources;

		AdapterSources( ImmutableArray<IAdapterSource> sources )
		{
			this.sources = sources;
		}

		public override IAdapterSource Get( Type parameter )
		{
			foreach ( var source in sources )
			{
				if ( source.IsSatisfiedBy( parameter ) )
				{
					return source;
				}
			}
			return null;
		}
	}
}