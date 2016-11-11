using DragonSpark.Aspects.Build;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Relay
{
	public abstract class SpecificationRelayAspectBase : ApplyRelayAspectBase
	{
		protected SpecificationRelayAspectBase( Func<object, IAspect> factory, IAspectBuildDefinition definition ) : base( factory, definition ) {}

		protected SpecificationRelayAspectBase() {}
	}
}