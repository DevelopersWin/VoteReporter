using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
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

		public override bool CompileTimeValidate( Type type ) => definition.IsSatisfiedBy( type );
		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => definition.GetEnumerable( (TypeInfo)targetElement );
	}
}