using DragonSpark.Extensions;
using Microsoft.Practices.ObjectBuilder2;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ArrayResolutionStrategy : Microsoft.Practices.Unity.ArrayResolutionStrategy
	{
		readonly Func<Type, object> provider;

		public ArrayResolutionStrategy( Func<Type, object> provider )
		{
			this.provider = provider;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			var hasBuildPlan = context.HasBuildPlan();
			if ( !hasBuildPlan )
			{
				base.PreBuildUp( context );

				if ( context.BuildComplete )
				{
					var array = context.Existing as Array;
					if ( array != null )
					{
						context.Complete( array.Length > 0 ? array : provider( context.BuildKey.Type ) ?? array );
					}
				}
			}
		}
	}
}