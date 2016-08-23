using DragonSpark.Commands;
using DragonSpark.Sources.Parameterized;
using System.Collections.Immutable;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class AutoValidation
	{
		public static ImmutableArray<IAdapterSource> DefaultSources { get; } =
			new IAdapterSource[]
			{
				new AdapterSource( typeof(IValidatedParameterizedSource<,>), GenericSourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(IValidatedParameterizedSource), SourceAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand<>), GenericCommandAdapterFactory.Default.Get ),
				new AdapterSource( typeof(ICommand), CommandAdapterFactory.Default.Get ),
			}.ToImmutableArray();
	}
}
