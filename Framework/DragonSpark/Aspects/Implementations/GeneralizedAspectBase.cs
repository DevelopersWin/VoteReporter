using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;

namespace DragonSpark.Aspects.Implementations
{
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class GeneralizedAspectBase : InstanceLevelAspect
	{
		readonly Func<object, IAspect> instanceFactory;

		protected GeneralizedAspectBase() : this( o => default(IAspect) ) {}

		protected GeneralizedAspectBase( Func<object, IAspect> instanceFactory )
		{
			this.instanceFactory = instanceFactory;
		}

		public override object CreateInstance( AdviceArgs adviceArgs ) => instanceFactory( adviceArgs.Instance );
	}
}