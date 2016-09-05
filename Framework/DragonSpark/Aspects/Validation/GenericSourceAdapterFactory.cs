using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public sealed class GenericSourceAdapterFactory : GenericParameterProfileFactoryBase
	{
		public static GenericSourceAdapterFactory Default { get; } = new GenericSourceAdapterFactory();
		GenericSourceAdapterFactory() : base( typeof(IParameterizedSource<,>), typeof(GenericSourceAdapterFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<TParameter, TResult>( ISpecification<TParameter> instance ) => new SourceAdapter<TParameter, TResult>( instance );
	}
}