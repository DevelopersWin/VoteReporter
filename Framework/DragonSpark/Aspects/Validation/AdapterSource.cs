using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	class AdapterSource : DelegatedParameterizedSource<object, IParameterValidationAdapter>, IAdapterSource
	{
		readonly TypeAdapter adapter;

		public AdapterSource( Type declaringType, Func<object, IParameterValidationAdapter> source ) : this( declaringType.Adapt(), source ) {}

		public AdapterSource( TypeAdapter adapter, Func<object, IParameterValidationAdapter> source ) : base( source )
		{
			this.adapter = adapter;
		}

		public bool IsSatisfiedBy( Type parameter ) => adapter.IsAssignableFrom( parameter );
		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}
}