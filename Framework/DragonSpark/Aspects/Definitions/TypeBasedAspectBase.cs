using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Aspects.Definitions
{
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class TypeBasedAspectBase : TypeLevelAspect, IAspectProvider
	{
		readonly IAspectBuildDefinition definition;

		protected TypeBasedAspectBase() {}

		protected TypeBasedAspectBase( IAspectBuildDefinition definition )
		{
			this.definition = definition;
		}

		public override bool CompileTimeValidate( Type type )
		{
			var result = definition.IsSatisfiedBy( type );
			if ( !result )
			{
				throw new InvalidOperationException( $"Aspect '{GetType()}' was applied to {type}, but it does not implement any of the types required by the applied aspect, which are: {string.Join( ", ", definition.Select( t => t.FullName ) )}" );
			}
			return true;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => definition.ProvideAspects( targetElement ).Fixed();
	}
}