using DragonSpark.Aspects.Validation;

namespace DragonSpark.Sources.Parameterized
{
	class AutoValidatingSource : AutoValidatingSourceBase<object, object>, IValidatedParameterizedSource
	{
		public AutoValidatingSource( IValidatedParameterizedSource inner ) : base( new AutoValidationController( new FactoryAdapter( inner ) ), inner.IsSatisfiedBy, inner.Get ) {}}
}