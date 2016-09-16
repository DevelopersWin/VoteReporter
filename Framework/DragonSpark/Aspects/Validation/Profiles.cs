using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Validation
{
	sealed class Profiles : ParameterizedSourceBase<Type, IAutoValidationProfile>
	{
		public static IParameterizedSource<Type, IAutoValidationProfile> Default { get; } = new Profiles().ToCache();
		Profiles() : this( Defaults.Profiles.OfType<IAutoValidationProfile>().ToImmutableArray() ) {}

		readonly ImmutableArray<IAutoValidationProfile> profiles;

		Profiles( ImmutableArray<IAutoValidationProfile> profiles )
		{
			this.profiles = profiles;
		}

		public override IAutoValidationProfile Get( Type parameter )
		{
			foreach ( var source in profiles )
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