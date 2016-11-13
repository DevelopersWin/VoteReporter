using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"{GetType()}.CompileTimeValidate: {type} => {result.ToString()}", null, null, null ));
			return result;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var type = (TypeInfo)targetElement;
			var result = definition.ProvideAspects( targetElement )?.ToArray();
			if ( result == null )
			{
				throw new InvalidOperationException( $"Aspect '{GetType()}' was applied to {targetElement}, but it was not able to apply any aspects to it.  Ensure that {targetElement} implements at least one of these types: {string.Join( ", ", definition.Select( t => t.FullName ) )}" );
			}
			foreach ( var aspectInstance in result )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"{GetType().Name}.ProvideAspects: [{type.FullName}] Applying {aspectInstance.AspectTypeName} => {aspectInstance.TargetElement}", null, null, null ));
			}
			return result;
		}
	}
}