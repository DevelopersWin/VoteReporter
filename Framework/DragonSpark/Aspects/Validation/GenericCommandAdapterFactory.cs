using DragonSpark.Aspects.Adapters;
using DragonSpark.Commands;

namespace DragonSpark.Aspects.Validation
{
	sealed class GenericCommandAdapterFactory : GenericAdapterFactory<object, IParameterValidationAdapter>
	{
		public static GenericCommandAdapterFactory Default { get; } = new GenericCommandAdapterFactory();
		GenericCommandAdapterFactory() : base( typeof(ICommand<>), typeof(CommandAdapter<>) ) {}
	}
}