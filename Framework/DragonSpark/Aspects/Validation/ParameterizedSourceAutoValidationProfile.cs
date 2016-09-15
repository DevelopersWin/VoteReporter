using System;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	sealed class ParameterizedSourceAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(IParameterizedSource<,>);

		public static ParameterizedSourceAutoValidationProfile Default { get; } = new ParameterizedSourceAutoValidationProfile();
		ParameterizedSourceAutoValidationProfile() : base( Type, Aspects.Defaults.Specification, new MethodDefinition( Type, nameof(ISource.Get) ) ) {}
	}
}