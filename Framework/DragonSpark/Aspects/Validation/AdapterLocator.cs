using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	sealed class AdapterLocator : FirstSelector<object, IParameterValidationAdapter>
	{
		public static AdapterLocator Default { get; } = new AdapterLocator();
		AdapterLocator() : base( ParameterizedSourceAdapterFactory.Default, GenericCommandAdapterFactory.Default, CommandAdapterFactory.Default ) {}
	}
}