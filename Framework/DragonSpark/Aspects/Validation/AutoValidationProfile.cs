using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Aspects.Validation
{
	public interface IAutoValidationProfile : IProfile, IParameterizedSource<IParameterValidationAdapter>, ISpecification<Type> {}

	public class AutoValidationProfile : Profile, IAutoValidationProfile
	{
		readonly TypeAdapter adapter;
		readonly Func<object, IParameterValidationAdapter> adapterSource;

		protected AutoValidationProfile( Type declaringType, IMethodLocator validation, IMethodLocator execution, Func<object, IParameterValidationAdapter> adapterSource )
			: this( declaringType.Adapt(), validation, execution, adapterSource ) {}

		AutoValidationProfile( TypeAdapter adapter, IMethodLocator validation, IMethodLocator execution, Func<object, IParameterValidationAdapter> adapterSource )
			: base( adapter.Type, new AspectInstance<AutoValidationValidationAspect>( validation ), new AspectInstance<AutoValidationExecuteAspect>( execution ) )
		{
			this.adapter = adapter;
			this.adapterSource = adapterSource;
		}

		public bool IsSatisfiedBy( Type parameter ) => adapter.IsAssignableFrom( parameter );

		public IParameterValidationAdapter Get( object parameter ) => adapterSource( parameter );
	}
}