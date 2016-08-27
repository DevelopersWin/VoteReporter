using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Sources.Delegates;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public static class Defaults
	{
		public static ISpecification<Type> KnownSourcesSpecification { get; } = IsSourceSpecification.Default.Or( IsParameterizedSourceSpecification.Default ).ToCachedSpecification();
		
		public static ISpecification<Type> ActivateSpecification { get; } = CanInstantiateSpecification.Default.Or( ContainsSingletonSpecification.Default ).ToCachedSpecification();
	
	}
}