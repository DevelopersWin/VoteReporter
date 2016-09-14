using DragonSpark.Commands;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Collections.Immutable;
using System.Windows.Input;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IAdapterSource> DefaultSources { get; } =
			new IAdapterSource[]
			{
				new AdapterSource( typeof(IParameterizedSource<,>), GenericSourceAdapterFactory.Default.Get ),
				// new AdapterSource( typeof(IValidatedParameterizedSource), SourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand<>), GenericCommandAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand), CommandAdapterFactory.Default.Get ),
			}.ToImmutableArray();

		public static ImmutableArray<IAspectProfile> DefaultProfiles { get; } = 
			new IAspectProfile[]
			{
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(IParameterizedSource<,>), nameof(ISource.Get) ) ),
				// new AspectProfile( typeof(IValidatedParameterizedSource), typeof(IParameterizedSource), nameof(IParameterizedSource.Get) ),
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(ICommand<>), nameof(ICommand.Execute) ) ),
				new AspectProfile( new Aspects.Extensions.MethodDefinition( typeof(ICommand), nameof(ICommand.Execute) ), new Aspects.Extensions.MethodDefinition( typeof(ICommand), nameof(ICommand.CanExecute) ) )
			}.ToImmutableArray();
	}
}
