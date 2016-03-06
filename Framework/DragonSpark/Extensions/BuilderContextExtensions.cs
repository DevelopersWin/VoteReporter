using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace DragonSpark.Extensions
{
	public static class BuilderContextExtensions
	{
		public static T New<T>( this ExtensionContext @this ) => (T)new BuilderContext( @this.Strategies.MakeStrategyChain(), @this.Lifetime, @this.Policies, NamedTypeBuildKey.Make<T>(), null ).With( context => context.Strategies.ExecuteBuildUp( context ) );

		public static bool HasBuildPlan( this IBuilderContext @this, NamedTypeBuildKey key = null ) => @this.Policies.GetNoDefault<IBuildPlanPolicy>( key ?? @this.BuildKey, false ) != null;

		// public static bool IsBuilding<T>( this IBuilderContext @this ) => IsBuilding( @this, NamedTypeBuildKey.Make<T>() );

		// public static bool IsBuilding( this IBuilderContext @this, NamedTypeBuildKey key ) => @this.GetCurrentBuildChain().Contains( key );

		// public static NamedTypeBuildKey[] GetCurrentBuildChain( this IBuilderContext @this ) => Ambient.GetCurrentChain<NamedTypeBuildKey>();

		public static T New<T>( this IBuilderContext @this, string name = null )
		{
			using ( new AmbientContextCommand<NamedTypeBuildKey>().ExecuteWith( NamedTypeBuildKey.Make<T>( name ) ) )
			{
				return @this.NewBuildUp<T>( name );
			}
		}

		public static void Complete( this IBuilderContext @this, object result )
		{
			@this.Existing = result;
			@this.BuildComplete = true;
		}

		public static void ClearBuildPlan( this IPolicyList @this, NamedTypeBuildKey key )
		{
			DependencyResolverTrackerPolicy.RemoveResolvers( @this, key );
			@this.Clear<IBuildPlanPolicy>( key );
		}
	}
}