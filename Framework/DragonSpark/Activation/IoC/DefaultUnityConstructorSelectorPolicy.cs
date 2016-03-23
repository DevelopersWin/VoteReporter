using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System;
using System.Linq;
using System.Reflection;
using DragonSpark.Aspects;

namespace DragonSpark.Activation.IoC
{
	public class DefaultUnityConstructorSelectorPolicy : Microsoft.Practices.Unity.ObjectBuilder.DefaultUnityConstructorSelectorPolicy
	{
		public static DefaultUnityConstructorSelectorPolicy Instance { get; } = new DefaultUnityConstructorSelectorPolicy();

		// [Freeze]
		protected override IDependencyResolverPolicy CreateResolver( ParameterInfo parameter )
		{
			var isOptional = parameter.IsOptional && !parameter.IsDefined( typeof(OptionalDependencyAttribute) );
			var result = isOptional ? 
				parameter.ParameterType.GetTypeInfo().IsValueType || parameter.ParameterType == typeof(string) 
					?
					(IDependencyResolverPolicy)new LiteralValueDependencyResolverPolicy( parameter.DefaultValue ) 
					: 
					new OptionalDependencyResolverPolicy( parameter.ParameterType ) 
				: Create( parameter );
			return result;
		}

		static IDependencyResolverPolicy Create( ParameterInfo parameter )
		{
			var attributes = parameter.GetCustomAttributes( false ) ?? Enumerable.Empty<Attribute>();
			var list = attributes.OfType<DependencyResolutionAttribute>().ToList();
			var result = list.Any() ? list.First().CreateResolver( parameter.ParameterType ) : new NamedTypeDependencyResolverPolicy( parameter.ParameterType, null );
			return result;
		}
	}
}