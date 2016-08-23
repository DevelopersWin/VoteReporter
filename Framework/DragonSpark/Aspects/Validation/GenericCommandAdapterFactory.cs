using DragonSpark.Commands;

namespace DragonSpark.Aspects.Validation
{
	sealed class GenericCommandAdapterFactory : GenericParameterProfileFactoryBase
	{
		public static GenericCommandAdapterFactory Default { get; } = new GenericCommandAdapterFactory();

		GenericCommandAdapterFactory() : base( typeof(ICommand<>), typeof(GenericCommandAdapterFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}
}