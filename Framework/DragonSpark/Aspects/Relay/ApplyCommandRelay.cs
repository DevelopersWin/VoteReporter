﻿using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ICommandRelay) )]
	public sealed class ApplyCommandRelay : SpecificationRelayAspectBase, ICommandRelay
	{
		readonly ICommandRelay relay;

		public ApplyCommandRelay() : base( ApplyCommandRelayDefinition.Default ) {}

		[UsedImplicitly]
		public ApplyCommandRelay( ICommandRelay relay ) : base( relay )
		{
			this.relay = relay;
		}

		public void Execute( object parameter ) => relay.Execute( parameter );
	}
}