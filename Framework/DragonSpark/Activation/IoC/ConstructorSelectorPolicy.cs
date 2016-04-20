using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Microsoft.Practices.Unity.Utility;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class DefaultUnityConstructorSelectorPolicy : Microsoft.Practices.Unity.ObjectBuilder.DefaultUnityConstructorSelectorPolicy
	{
		readonly Func<ParameterInfo, IDependencyResolverPolicy> resolver;
		public static DefaultUnityConstructorSelectorPolicy Instance { get; } = new DefaultUnityConstructorSelectorPolicy();

		public DefaultUnityConstructorSelectorPolicy() : this( ResolverFactory.Instance.Create ) {}

		public DefaultUnityConstructorSelectorPolicy( Func<ParameterInfo, IDependencyResolverPolicy> resolver )
		{
			this.resolver = resolver;
		}

		protected override IDependencyResolverPolicy CreateResolver( ParameterInfo parameter ) => resolver( parameter );
	}

	class ResolverFactory : FactoryBase<ParameterInfo, IDependencyResolverPolicy>
	{
		public static ResolverFactory Instance { get; } = new ResolverFactory();

		protected override IDependencyResolverPolicy CreateItem( ParameterInfo parameter )
		{
			var isOptional = parameter.IsOptional && !parameter.IsDefined( typeof(OptionalDependencyAttribute) );
			var result = isOptional ? 
				parameter.ParameterType.GetTypeInfo().IsValueType || parameter.ParameterType == typeof(string) 
					?
					(IDependencyResolverPolicy)new LiteralValueDependencyResolverPolicy( parameter.DefaultValue ) 
					: 
					new OptionalDependencyResolverPolicy( parameter.ParameterType ) 
				: CreateFrom( parameter );
			return result;
		}

		static IDependencyResolverPolicy CreateFrom( ParameterInfo parameter )
		{
			var attributes = parameter.GetCustomAttributes( false ) ?? Enumerable.Empty<Attribute>();
			var list = attributes.OfType<DependencyResolutionAttribute>().ToList();
			var result = list.Any() ? list.First().CreateResolver( parameter.ParameterType ) : new NamedTypeDependencyResolverPolicy( parameter.ParameterType, null );
			return result;
		}
	}

	class ConstructorSelectorPolicy : IConstructorSelectorPolicy
	{
		readonly Func<ISpecification<TypeRequest>> source;
		// public static ConstructorSelectorPolicy Instance { get; } = new ConstructorSelectorPolicy();

		public ConstructorSelectorPolicy( Func<ISpecification<TypeRequest>> source )
		{
			this.source = source;
		}

		public SelectedConstructor SelectConstructor( IBuilderContext context, IPolicyList resolverPolicyDestination ) => Create( context.BuildKey.Type ) ?? DefaultUnityConstructorSelectorPolicy.Instance.SelectConstructor( context, resolverPolicyDestination );

		[Freeze]
		SelectedConstructor Create( Type type )
		{
			var ctor = FromMetadata( type ) ?? Search( type );
			var result = ctor.With( CreateSelectedConstructor );
			return result;
		}

		static SelectedConstructor CreateSelectedConstructor( ConstructorInfo ctor )
		{
			var result = new SelectedConstructor( ctor );
			foreach ( var parameter in ctor.GetParameters() )
				result.AddParameterResolver( ResolverFactory.Instance.Create( parameter ) );
			return result;
		}

		private static ConstructorInfo FromMetadata( Type typeToConstruct )
		{
			var array = new ReflectionHelper( typeToConstruct ).InstanceConstructors.Where( ctor => ctor.IsDefined( typeof(InjectionConstructorAttribute), true ) ).ToArray();
			switch ( array.Length )
			{
				case 0:
					return null;
				case 1:
					return array[0];
				default:
					throw new InvalidOperationException( string.Format( CultureInfo.CurrentCulture, "Resources.MultipleInjectionConstructors: {0}", (object)typeToConstruct.GetTypeInfo().Name ) );
			}
		}

		ConstructorInfo Search( Type typeToConstruct )
		{
			var result = source.Use( specification =>
			{
				var constructors = new ReflectionHelper( typeToConstruct ).InstanceConstructors;
				var constructor = constructors
					.OrderByDescending( info => info.GetParameters().Length )
					.FirstOrDefault( info => 
						info.GetParameters().With( infos => !infos.Any() || infos.Select( parameterInfo => new LocateTypeRequest( parameterInfo.ParameterType ) ).All( specification.IsSatisfiedBy ) ) 
						);
				return constructor;
			} );
			return result;
		}
	}
}