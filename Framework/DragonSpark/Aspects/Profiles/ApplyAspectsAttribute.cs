using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Coercion;
using DragonSpark.Aspects.Relay;
using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Coercion;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Profiles
{
	[AttributeUsage( AttributeTargets.Class ), LinesOfCodeAvoided( 10 )]
	public class ApplyAspectsAttribute : Attribute, IAspectProvider
	{
		readonly ImmutableArray<ObjectConstruction> constructions;

		public ApplyAspectsAttribute( Type coercerType, Type specificationType ) : this( new ConstructionsSource( coercerType, specificationType ).Get() ) {}
		public ApplyAspectsAttribute( params ObjectConstruction[] aspects ) : this( aspects.ToImmutableArray() ) {}
		public ApplyAspectsAttribute( ImmutableArray<ObjectConstruction> constructions )
		{
			this.constructions = constructions;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => 
			constructions.Introduce( targetElement, tuple => new AspectInstance( tuple.Item2, tuple.Item1, null ) );
	}

	class ConstructionsSource : ItemSource<ObjectConstruction>
	{
		readonly static ObjectConstruction Relay = ObjectConstructionFactory<ApplyRelayAttribute>.Default.Get();
		readonly static ObjectConstruction Auto = ObjectConstructionFactory<ApplyAutoValidationAttribute>.Default.Get();
		readonly static ObjectConstructionFactory<ApplyCoercerAttribute> Coercer = ObjectConstructionFactory<ApplyCoercerAttribute>.Default;
		readonly static ObjectConstructionFactory<ApplySpecificationAttribute> Specification = ObjectConstructionFactory<ApplySpecificationAttribute>.Default;

		public ConstructionsSource( [OfType( typeof(ICoercer<,>) )]Type coercerType, [OfType( typeof(ISpecification<>) )]Type specificationType )
			: base( 
				// new ApplyGeneralizedImplementationsAttribute(),
				Coercer.GetUsing( coercerType ),
				Relay,
				Auto,
				Specification.GetUsing( specificationType )
			) {}
	}
}