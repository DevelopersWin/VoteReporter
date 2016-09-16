using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Validation
{
	static class Defaults
	{
		public static ImmutableArray<IProfile> Profiles { get; } = 
			ImmutableArray.Create<IProfile>( ParameterizedSourceAutoValidationProfile.Default, GenericCommandAutoValidationProfile.Default, CommandAutoValidationProfile.Default );
			
		public static ImmutableArray<TypeAdapter> Adapters { get; } = Profiles.Select( profile => profile.DeclaringType.Adapt() ).ToImmutableArray();

		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Default.Get;
	}
}