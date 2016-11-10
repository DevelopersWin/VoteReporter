using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class ApplyInverseSpecificationAttribute : SpecificationAspectBase
	{
		public ApplyInverseSpecificationAttribute( Type specificationType ) : base( Factory<ApplyInverseSpecificationAttribute>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplyInverseSpecificationAttribute( ISpecificationAdapter specification ) : base( new SpecificationAdapter( specification.To( CastCoercer<bool>.Default ) ) ) {}

		sealed class SpecificationAdapter : AdapterBase<object, bool>, ISpecificationAdapter
		{
			readonly IParameterizedSource<object, bool> specification;

			public SpecificationAdapter( IParameterizedSource<object, bool> specification )
			{
				this.specification = specification;
			}

			public override bool Get( object parameter ) => !specification.Get( parameter );
		}
	}
}