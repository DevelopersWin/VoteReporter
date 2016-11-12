using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects
{
	public abstract class InstanceAspectBase : TypeBasedAspectBase, IInstanceScopedAspect
	{
		readonly Func<object, IAspect> factory;

		protected InstanceAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( definition )
		{
			this.factory = factory;
		}

		protected InstanceAspectBase() {}

		public object CreateInstance( AdviceArgs adviceArgs ) => factory( adviceArgs.Instance );

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}
}