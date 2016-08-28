using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Sources.Delegates;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public static class Defaults<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Default.Coerce;
	}

	public static class Defaults
	{
		public static ISpecification<Type> KnownSourcesSpecification { get; } = IsSourceSpecification.Default.Or( IsParameterizedSourceSpecification.Default ).ToCachedSpecification();
		
		public static ISpecification<Type> ProvidesInstanceSpecification { get; } = CanActivateSpecification.Default.Or( ContainsSingletonPropertySpecification.Default ).ToCachedSpecification();
	
	}
}