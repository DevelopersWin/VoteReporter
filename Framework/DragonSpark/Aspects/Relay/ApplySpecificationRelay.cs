using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<ISpecificationRelayAdapter>) )]
	public sealed class ApplySpecificationRelay : ApplySpecificationRelayBase
	{
		public ApplySpecificationRelay()  : base( SpecificationSelectors.Default.Get, SpecificationRelayDefinition.Default ) {}
		public ApplySpecificationRelay( ISpecificationRelayAdapter relay ) : base( relay ) {}
	}

	[UsedImplicitly]
	public abstract class ApplySpecificationRelayBase : InstanceAspectBase, ISource<ISpecificationRelayAdapter>
	{
		readonly ISpecificationRelayAdapter relay;

		protected ApplySpecificationRelayBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		[UsedImplicitly]
		protected ApplySpecificationRelayBase( ISpecificationRelayAdapter relay )
		{
			this.relay = relay;
		}

		public ISpecificationRelayAdapter Get() => relay;
	}


}