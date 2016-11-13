using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Coercion;
using DragonSpark.Aspects.Implementations;
using DragonSpark.Aspects.Relay;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Reflection;
using System;

namespace DragonSpark.Aspects.Adapters
{
	sealed class ConstructionsSource : ItemSource<ObjectConstruction>
	{
		readonly static ObjectConstruction 
			Implementations = ObjectConstructionFactory<ApplyGeneralizedImplementations>.Default.Get(),
			Relay = ObjectConstructionFactory<ApplyRelays>.Default.Get(), 
			Auto = ObjectConstructionFactory<ApplyAutoValidation>.Default.Get();
		readonly static ObjectConstructionFactory<ApplyCoercer> Coercer = ObjectConstructionFactory<ApplyCoercer>.Default;
		readonly static ObjectConstructionFactory<ApplySpecification> Specification = ObjectConstructionFactory<ApplySpecification>.Default;

		public ConstructionsSource( [OfType( typeof(IParameterizedSource<,>) )]Type coercerType, [OfType( typeof(ISpecification<>) )]Type specificationType )
			: base(
				Implementations,
				Coercer.Get( coercerType ),
				Relay,
				Auto,
				Specification.Get( specificationType )
			) {}
	}
}