using DragonSpark.Runtime.Values;
using Microsoft.Practices.ObjectBuilder2;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Extensions
{
	public static class BuilderContextExtensions
	{
		/*public static BuilderContext RelayParameterAware<T>( this ExtensionContext @this ) => new BuilderContext( @this.Strategies.MakeStrategyChain(), @this.Lifetime, @this.Policies, NamedTypeBuildKey.Make<T>(), null );

		public static T New<T>( this ExtensionContext @this ) => @this.RelayParameterAware<T>().With( context => context.New<T>() );*/

		/*public static T AddStrategy<T>( this ExtensionContext @this, UnityBuildStage stage ) where T : IBuilderStrategy
		{
			var result = @this.New<T>();
			@this.Strategies.Add( result, stage );
			return result;
		}*/

		// public static bool HasRegisteredBuildPlan( this IBuilderContext @this, NamedTypeBuildKey key = null ) => @this.GetBuildPlan( key ).With( policy => !new DefaultInjection.Applied( policy ).Item.IsApplied );

		public static bool HasBuildPlan( this IBuilderContext @this, NamedTypeBuildKey key = null ) => @this.GetBuildPlan( key ) != null;

		public static IBuildPlanPolicy GetBuildPlan( this IBuilderContext @this, NamedTypeBuildKey key = null ) => @this.Policies.GetBuildPlan( key ?? @this.BuildKey );
		
		// public static bool IsBuilding<T>( this IBuilderContext @this ) => IsBuilding( @this, NamedTypeBuildKey.Make<T>() );

		// public static bool IsBuilding( this IBuilderContext @this, NamedTypeBuildKey key ) => @this.GetCurrentBuildChain().Contains( key );

		public static NamedTypeBuildKey[] GetCurrentBuildChain( this IBuilderContext @this ) => Ambient.GetCurrentChain<NamedTypeBuildKey>();

		/*public static T New<T>( this IBuilderContext @this, string name = null )
		{
			using ( new AmbientContextCommand<NamedTypeBuildKey>().ExecuteWith( NamedTypeBuildKey.Make<T>( name ) ) )
			{
				return @this.NewBuildUp<T>( name );
			}
		}*/

		public static void Complete( this IBuilderContext @this, object result )
		{
			@this.Existing = result;
			@this.BuildComplete = result != null;
		}

		// public static bool HasBuildPlan( this IPolicyList @this, NamedTypeBuildKey key ) => GetBuildPlan( @this, key ) != null;

		public static IBuildPlanPolicy GetBuildPlan( this IPolicyList @this, [Required]NamedTypeBuildKey key ) => @this.GetNoDefault<IBuildPlanPolicy>( key, false ).With( policy => policy.GetType().Name != "OverriddenBuildPlanMarkerPolicy" ? policy : null );

		public static void ClearBuildPlan( this IPolicyList @this, NamedTypeBuildKey key )
		{
			DependencyResolverTrackerPolicy.RemoveResolvers( @this, key );
			@this.Clear<IBuildPlanPolicy>( key );
		}
	}
}