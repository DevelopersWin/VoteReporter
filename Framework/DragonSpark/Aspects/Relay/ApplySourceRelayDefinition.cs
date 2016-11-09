﻿using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Relay
{
	public sealed class ApplySourceRelayDefinition : AspectBuildDefinition<IParameterizedSourceRelay, ApplyParameterizedSourceRelay>
	{
		public static ApplySourceRelayDefinition Default { get; } = new ApplySourceRelayDefinition();
		ApplySourceRelayDefinition() : base( 
			GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType, ParameterizedSourceTypeDefinition.Default.ReferencedType, 
			typeof(ParameterizedSourceRelayAdapter<,>),
			new MethodAspectSelector<ParameterizedSourceMethodAspect>( GeneralizedParameterizedSourceTypeDefinition.Default.Method )
		) {}
	}
}