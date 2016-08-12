using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class DefaultUnityConstructorSelectorPolicy : Microsoft.Practices.Unity.ObjectBuilder.DefaultUnityConstructorSelectorPolicy
	{
		public static DefaultUnityConstructorSelectorPolicy Instance { get; } = new DefaultUnityConstructorSelectorPolicy();
		DefaultUnityConstructorSelectorPolicy() : this( ResolverFactory.Instance.ToDelegate() ) {}

		readonly Func<ParameterInfo, IDependencyResolverPolicy> resolver;

		public DefaultUnityConstructorSelectorPolicy( Func<ParameterInfo, IDependencyResolverPolicy> resolver )
		{
			this.resolver = resolver;
		}

		protected override IDependencyResolverPolicy CreateResolver( ParameterInfo parameter ) => resolver( parameter );
	}

	class ResolverFactory : FactoryBase<ParameterInfo, IDependencyResolverPolicy>
	{
		public static ResolverFactory Instance { get; } = new ResolverFactory();
		ResolverFactory() {}

		public override IDependencyResolverPolicy Create( ParameterInfo parameter )
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

	public class ConstructorSelectorPolicy : IConstructorSelectorPolicy
	{
		readonly static Func<ParameterInfo, IDependencyResolverPolicy> Resolver = ResolverFactory.Instance.ToDelegate();
		readonly Func<ConstructTypeRequest, ConstructorInfo> locator;
		readonly Func<ParameterInfo, IDependencyResolverPolicy> resolver;
		readonly Func<ConstructorInfo, SelectedConstructor> createDelegate;

		public ConstructorSelectorPolicy( ConstructorLocator locator ) : this( locator.ToDelegate(), Resolver ) {}

		protected ConstructorSelectorPolicy( Func<ConstructTypeRequest, ConstructorInfo> locator, Func<ParameterInfo, IDependencyResolverPolicy> resolver )
		{
			this.locator = locator;
			this.resolver = resolver;
			createDelegate = CreateSelectedConstructor;
		}

		public SelectedConstructor SelectConstructor( IBuilderContext context, IPolicyList resolverPolicyDestination ) => 
			locator( new ConstructTypeRequest( context.BuildKey.Type ) ).With( createDelegate ) ?? DefaultUnityConstructorSelectorPolicy.Instance.SelectConstructor( context, resolverPolicyDestination );

		SelectedConstructor CreateSelectedConstructor( ConstructorInfo ctor )
		{
			var result = new SelectedConstructor( ctor );
			ctor.GetParameters().Select( resolver ).Each( result.AddParameterResolver );
			return result;
		}
	}
}