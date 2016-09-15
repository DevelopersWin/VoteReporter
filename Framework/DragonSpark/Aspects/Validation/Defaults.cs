using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	static class Defaults
	{
		public static ImmutableArray<IProfile> Profiles { get; } = 
			ImmutableArray.Create<IProfile>( ParameterizedSourceAutoValidationProfile.Default, GenericCommandAutoValidationProfile.Default, CommandAutoValidationProfile.Default );
			
		public static ImmutableArray<TypeAdapter> Adapters { get; } = Profiles.Select( profile => profile.DeclaringType.Adapt() ).ToImmutableArray();

		/*public static ImmutableArray<IAspectProfile> DefaultProfiles { get; } = 
			new IAspectProfile[]
			{
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(IParameterizedSource<,>), nameof(ISource.Get) ) ),
				// new AspectProfile( typeof(IValidatedParameterizedSource), typeof(IParameterizedSource), nameof(IParameterizedSource.Get) ),
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(ICommand<>), nameof(ICommand.Execute) ) ),
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(ICommand), nameof(ICommand.Execute) ), new Aspects.Extensions.MethodDefinition( typeof(ICommand), nameof(ICommand.CanExecute) ) )
			}.ToImmutableArray();*/

		public static Func<object, IAutoValidationController> ControllerSource { get; } = AutoValidationControllerFactory.Default.Get;

		public static ImmutableArray<IAdapterSource> Sources { get; } =
			new IAdapterSource[]
			{
				new AdapterSource( typeof(IParameterizedSource<,>), GenericSourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand<>), GenericCommandAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand), CommandAdapterFactory.Default.Get ),
			}.ToImmutableArray();
	}
}