using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	public class BuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
	{
		readonly Func<Action, Exception> tryDelegate;
		readonly IList<IBuildPlanPolicy> policies;
		readonly IBuildPlanCreatorPolicy[] creators;

		public BuildPlanCreatorPolicy( [Required]Func<Action, Exception> tryDelegate, [Required]IList<IBuildPlanPolicy> policies, [Required]params IBuildPlanCreatorPolicy[] creators )
		{
			this.tryDelegate = tryDelegate;
			this.policies = policies;
			this.creators = creators;
		}

		public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey ) => 
			new CompositeBuildPlanPolicy( tryDelegate, creators.Select( policy => policy.CreatePlan( context, buildKey ) ).Concat( policies ).ToArray() );
	}

	public class CompositeBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly Func<Action, Exception> tryDelegate;
		readonly IBuildPlanPolicy[] policies;

		public CompositeBuildPlanPolicy( [Required]Func<Action, Exception> tryDelegate, params IBuildPlanPolicy[] policies )
		{
			this.tryDelegate = tryDelegate;
			this.policies = policies;
		}

		public void BuildUp( IBuilderContext context )
		{
			Exception first = null;
			foreach ( var exception in policies.Select( policy => tryDelegate( () => policy.BuildUp( context ) ) ) )
			{
				if ( exception == null && context.Existing != null )
				{
					return;
				}
				first = first ?? exception;
			}
			throw first;
		}
	}

	class SingletonBuildPlanPolicy : IBuildPlanPolicy
	{
		readonly ISingletonLocator locator;

		public SingletonBuildPlanPolicy() : this( SingletonLocator.Instance ) {}

		public SingletonBuildPlanPolicy( [Required] ISingletonLocator locator )
		{
			this.locator = locator;
		}

		public void BuildUp( IBuilderContext context )
		{
			var singleton = locator.Locate( context.BuildKey.Type );
			if ( singleton != null )
			{
				context.Existing = singleton;
			}
		}
	}

	public class EnumerableResolutionStrategy : BuilderStrategy
	{
		delegate object Resolver( IBuilderContext context );

		readonly static MethodInfo GenericResolveArrayMethod = typeof(EnumerableResolutionStrategy).GetTypeInfo().DeclaredMethods.First( m => m.Name == nameof(Resolve) && !m.IsPublic );

		readonly IUnityContainer container;

		public EnumerableResolutionStrategy( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		public override void PreBuildUp( [Required]IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				var adapt = context.BuildKey.Type.Adapt();
				if ( adapt.IsGenericOf<IEnumerable<object>>( false ) )
				{
					adapt.GetEnumerableType().With( type =>
					{
						var resolver = (Resolver)GenericResolveArrayMethod.MakeGenericMethod( type ).CreateDelegate( typeof(Resolver), this );

						var result = resolver( context );
						context.Complete( result );
					} );
				}
			}
		}

		object Resolve<T>( IBuilderContext context )
		{
			var defaultName = container.IsRegistered<T>() ? new string[] { null } : Default<string>.Items;
			var result = context.Policies.Get<IRegisteredNamesPolicy>( null )
				.With( policy => policy.GetRegisteredNames( typeof(T) )
					.Concat( defaultName ).Concat( typeof(T).GetTypeInfo().IsGenericType ? policy.GetRegisteredNames( typeof(T).GetGenericTypeDefinition() ) : Enumerable.Empty<string>() )
					.Distinct()
					.Select( context.New<T> )
					.ToArray() 
				) ?? Default<T>.Items;
			return result;
		}
	}
}
