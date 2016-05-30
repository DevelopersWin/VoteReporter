using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation.IoC
{
	public class BuildPlanCreatorPolicy : IBuildPlanCreatorPolicy
	{
		readonly Func<Action, Exception> tryDelegate;
		readonly ISpecification<LocateTypeRequest> specification;
		readonly IEnumerable<IBuildPlanPolicy> policies;
		readonly IBuildPlanCreatorPolicy[] creators;

		public BuildPlanCreatorPolicy( [Required] Func<Action, Exception> tryDelegate, ISpecification<LocateTypeRequest> specification, [Required] IEnumerable<IBuildPlanPolicy> policies, [Required] params IBuildPlanCreatorPolicy[] creators )
		{
			this.tryDelegate = tryDelegate;
			this.specification = specification;
			this.policies = policies;
			this.creators = creators;
		}

		public IBuildPlanPolicy CreatePlan( IBuilderContext context, NamedTypeBuildKey buildKey )
		{
			var plans = specification.IsSatisfiedBy( new LocateTypeRequest( buildKey.Type, buildKey.Name ) ) ? creators.Select( policy => policy.CreatePlan( context, buildKey ) ) : Items<IBuildPlanPolicy>.Default;
			var result = new CompositeBuildPlanPolicy( tryDelegate, plans.Concat( policies ).ToArray() );
			return result;
		}
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
		public static SingletonBuildPlanPolicy Instance { get; } = new SingletonBuildPlanPolicy();

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
		readonly IServiceProvider provider;

		public EnumerableResolutionStrategy( [Required]IUnityContainer container, IServiceProvider provider )
		{
			this.container = container;
			this.provider = provider;
		}

		public override void PreBuildUp( [Required]IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				BuildUp( context );

				if ( context.BuildComplete )
				{
					context.Existing.As<Array>( array =>
					{
						var result = array.Length > 0 ? array : provider.GetService( context.BuildKey.Type ) ?? array;
						context.Complete( result );
					} );
				}
			}
		}

		void BuildUp( IBuilderContext context )
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

		object Resolve<T>( IBuilderContext context )
		{
			var defaultName = container.IsRegistered<T>() ? new string[] { null } : Items<string>.Default;
			var result = context.Policies.Get<IRegisteredNamesPolicy>( null )
				.With( policy => policy.GetRegisteredNames( typeof(T) )
					.Concat( defaultName ).Concat( typeof(T).GetTypeInfo().IsGenericType ? policy.GetRegisteredNames( typeof(T).GetGenericTypeDefinition() ) : Enumerable.Empty<string>() )
					.Distinct()
					.Select( context.NewBuildUp<T> )
					.ToArray() 
				) ?? Items<T>.Default;
			return result;
		}
	}
}
