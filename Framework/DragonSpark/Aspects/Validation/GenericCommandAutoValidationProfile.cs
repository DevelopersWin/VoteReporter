using System;
using System.Windows.Input;
using DragonSpark.Commands;

namespace DragonSpark.Aspects.Validation
{
	sealed class GenericCommandAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(ICommand<>);

		public static GenericCommandAutoValidationProfile Default { get; } = new GenericCommandAutoValidationProfile();
		GenericCommandAutoValidationProfile() : base( Type, Aspects.Defaults.Specification, new MethodDefinition( Type, nameof(ICommand.Execute) ) ) {}
	}
}