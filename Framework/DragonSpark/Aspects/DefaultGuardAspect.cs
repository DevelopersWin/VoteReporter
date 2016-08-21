using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using PostSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[Serializable, MulticastAttributeUsage( MulticastTargets.Method | MulticastTargets.InstanceConstructor, TargetMemberAttributes = MulticastAttributes.NonAbstract ), LinesOfCodeAvoided( 1 )]
	public class DefaultGuardAspect : Aspect, IAspectProvider
	{
		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var info = targetElement as MethodInfo;
			if ( info != null && !info.IsSpecialName )
			{
				foreach ( var parameter in info.GetParameters() )
				{
					if ( !parameter.IsOptional && !parameter.IsOut && parameter.ParameterType.GetTypeInfo().IsClass )
					{
						var attribute = parameter.ParameterType == typeof(string) ? typeof(RequiredAttribute) : typeof(NotNullAttribute);
						yield return new AspectInstance( parameter, new ObjectConstruction( attribute ), null ) { RepresentAsStandalone = true };
						/*var optional = parameter.IsOptional/* || parameter.CustomAttributes.Any( a => a.AttributeType == typeof(OptionalAttribute) )#1#;
						if ( !optional )
						{
							
						}*/
					}
				}
			}
		}
	}
}
