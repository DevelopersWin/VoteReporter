using DragonSpark.Commands;

namespace DragonSpark.Aspects.Validation
{
	sealed class GenericCommandAdapterFactory : AdapterFactory<object, IParameterValidationAdapter>
	{
		public static GenericCommandAdapterFactory Default { get; } = new GenericCommandAdapterFactory();
		GenericCommandAdapterFactory() : base( typeof(ICommand<>), typeof(CommandAdapter<>) ) {}
	}
}