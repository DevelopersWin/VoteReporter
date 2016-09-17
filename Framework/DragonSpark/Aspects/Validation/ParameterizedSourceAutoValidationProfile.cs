using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Validation
{
	sealed class ParameterizedSourceAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(IParameterizedSource<,>);

		public static ParameterizedSourceAutoValidationProfile Default { get; } = new ParameterizedSourceAutoValidationProfile();
		ParameterizedSourceAutoValidationProfile() : base( Type, Aspects.Defaults.Specification, new MethodLocator( Type, nameof(ISource.Get) ), GenericSourceAdapterFactory.Default.Get ) {}
	}
}