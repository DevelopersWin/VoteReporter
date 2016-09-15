using System;

namespace DragonSpark.Aspects.Validation
{
	public class AutoValidationProfile : Profile
	{
		protected AutoValidationProfile( Type declaringType, IMethodLocator validation, IMethodLocator execution )
			: base( declaringType, new AspectSource<AutoValidationValidationAspect>( validation ), new AspectSource<AutoValidationExecuteAspect>( execution ) ) {}
	}
}