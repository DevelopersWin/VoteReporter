﻿using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ISource<ISpecificationRelayAdapter>) )]
	public sealed class ApplySpecificationRelay : InstanceAspectBase, ISource<ISpecificationRelayAdapter>
	{
		readonly ISpecificationRelayAdapter relay;

		public ApplySpecificationRelay()  : base( SpecificationFactory.Default.Get, SpecificationRelayDefinition.Default ) {}
		
		[UsedImplicitly]
		public ApplySpecificationRelay( ISpecificationRelayAdapter relay )
		{
			this.relay = relay;
		}

		public ISpecificationRelayAdapter Get() => relay;
	}
}