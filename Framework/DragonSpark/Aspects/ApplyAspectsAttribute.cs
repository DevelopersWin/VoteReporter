using DragonSpark.Extensions;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects
{
	[AttributeUsage( AttributeTargets.Class ), LinesOfCodeAvoided( 10 )]
	public class ApplyAspectsAttribute : Attribute, IAspectProvider
	{
		readonly ImmutableArray<ObjectConstruction> constructions;

		public ApplyAspectsAttribute( Type coercerType, Type specificationType ) : this( new ConstructionsSource( coercerType, specificationType ).Fixed() ) {}

		[UsedImplicitly]
		protected ApplyAspectsAttribute( params ObjectConstruction[] constructions )
		{
			this.constructions = constructions.ToImmutableArray();
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => 
			constructions.Introduce( targetElement, tuple => new AspectInstance( tuple.Item2, tuple.Item1, null ) );
	}
}