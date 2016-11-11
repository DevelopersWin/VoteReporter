using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISource<ISpecificationAdapter>) )]
	public sealed class ApplyInverseSpecification : SpecificationAspectBase, ISource<ISpecificationAdapter>, IAspectProvider
	{
		public ApplyInverseSpecification( Type specificationType ) : base( Factory<ApplyInverseSpecification>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplyInverseSpecification( ISpecificationAdapter specification ) : base( new SpecificationAdapter( specification.To( CastCoercer<bool>.Default ) ) ) {}

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