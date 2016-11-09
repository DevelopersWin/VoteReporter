using System;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Coercion;
using DragonSpark.Aspects.Relay;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Reflection;

namespace DragonSpark.Aspects.Adapters
{
	sealed class ConstructionsSource : ItemSource<ObjectConstruction>
	{
		readonly static ObjectConstruction Relay = ObjectConstructionFactory<ApplyRelaysAttribute>.Default.Get();
		readonly static ObjectConstruction Auto = ObjectConstructionFactory<ApplyAutoValidationAttribute>.Default.Get();
		readonly static ObjectConstructionFactory<ApplyCoercerAttribute> Coercer = ObjectConstructionFactory<ApplyCoercerAttribute>.Default;
		readonly static ObjectConstructionFactory<ApplySpecificationAttribute> Specification = ObjectConstructionFactory<ApplySpecificationAttribute>.Default;

		public ConstructionsSource( [OfType( typeof(IParameterizedSource<,>) )]Type coercerType, [OfType( typeof(ISpecification<>) )]Type specificationType )
			: base( 
				Coercer.Get( coercerType ),
				Relay,
				Auto,
				Specification.Get( specificationType )
			) {}
	}
}