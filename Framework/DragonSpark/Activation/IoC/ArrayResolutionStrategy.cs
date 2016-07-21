using DragonSpark.Extensions;
using DragonSpark.Setup;
using Microsoft.Practices.ObjectBuilder2;
using System;

namespace DragonSpark.Activation.IoC
{
	public class ArrayResolutionStrategy : Microsoft.Practices.Unity.ArrayResolutionStrategy
	{
		readonly IDependencyLocatorKey key;

		public ArrayResolutionStrategy( IDependencyLocatorKey key )
		{
			this.key = key;
		}

		public override void PreBuildUp( IBuilderContext context )
		{
			if ( !context.HasBuildPlan() )
			{
				base.PreBuildUp( context );

				if ( context.BuildComplete )
				{
					var array = context.Existing as Array;
					if ( array != null )
					{
						context.Complete( array.Length > 0 ? array : DependencyLocator.Instance.For( key )?.Invoke( context.BuildKey.Type ) ?? array );
					}
				}
			}
		}
	}
}