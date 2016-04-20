using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ArrayResolutionStrategy : Microsoft.Practices.Unity.ArrayResolutionStrategy
	{
		readonly IServiceProvider provider;

		public ArrayResolutionStrategy( IServiceProvider provider )
		{
			this.provider = provider;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				base.PreBuildUp( context );

				if ( context.BuildComplete )
				{
					context.Existing.As<Array>( array => context.Complete( array.Length > 0 ? array : provider.GetService( context.BuildKey.Type ) ?? array ) );
				}
			}
		}
	}
}