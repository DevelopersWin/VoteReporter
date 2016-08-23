using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[PSerializable, AttributeUsage( AttributeTargets.Method ), MulticastAttributeUsage( MulticastTargets.Method | MulticastTargets.InstanceConstructor, TargetMemberAttributes = MulticastAttributes.NonAbstract ), LinesOfCodeAvoided( 1 )]
	public class DefaultGuardAspect : MethodLevelAspect, IAspectProvider
	{
		public override bool CompileTimeValidate( MethodBase method ) => ( !method.IsSpecialName || method is ConstructorInfo ) && method.GetParameters().Any();

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var info = targetElement as MethodBase;
			if ( info != null )
			{
				foreach ( var parameter in info.GetParameters() )
				{
					var parameterType = parameter.ParameterType;
					if ( !parameterType.IsByRef && !parameter.IsOptional && Nullable.GetUnderlyingType( parameterType ) == null && !parameterType.GetTypeInfo().IsValueType )
					{
						
						var attribute = parameterType == typeof(string) ? typeof(RequiredAttribute) : typeof(NotNullAttribute);
						yield return new AspectInstance( parameter, new ObjectConstruction( attribute ), null ) { RepresentAsStandalone = true };
					}
				}
			}
		}
	}
}
