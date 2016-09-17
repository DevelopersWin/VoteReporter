using DragonSpark.Aspects.Build;
using DragonSpark.Commands;
using System;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	sealed class GenericCommandAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(ICommand<>);

		public static GenericCommandAutoValidationProfile Default { get; } = new GenericCommandAutoValidationProfile();
		GenericCommandAutoValidationProfile() : base( Type, Aspects.Defaults.Specification, new MethodLocator( Type, nameof(ICommand.Execute) ), GenericCommandAdapterFactory.Default.Get ) {}
	}
}