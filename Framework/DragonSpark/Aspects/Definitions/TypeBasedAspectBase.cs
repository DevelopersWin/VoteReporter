using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;

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

		public override bool CompileTimeValidate( Type type ) => definition.IsSatisfiedBy( type );
		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var aspectInstances = definition.ProvideAspects( targetElement ).Fixed();
			MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"{this}: {targetElement} = {aspectInstances.Length}", null, null, null ));
			foreach ( var aspectInstance in aspectInstances )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"     {this}: {targetElement} = - {aspectInstance.TargetElement} -- {aspectInstance.AspectConstruction.TypeName}", null, null, null ));
			}
			return aspectInstances;
		}
	}
}