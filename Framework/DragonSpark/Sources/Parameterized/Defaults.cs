using System;
using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Sources.Delegates;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public static class Defaults
	{
		public static ISpecification<Type> KnownSourcesSpecification { get; } = IsSourceSpecification.Default.Or( IsParameterizedSourceSpecification.Default ).ToCachedSpecification();
		
		public static ISpecification<Type> ActivateSpecification { get; } = CanInstantiateSpecification.Default.Or( ContainsSingletonSpecification.Default ).ToCachedSpecification();

		public static ISpecification<Type> IsExportSpecification { get; } = Composition.IsExportSpecification.Default.Project( Projections.MemberType ).Or( ContainsExportedSingletonSpecification.Default ).ToCachedSpecification();
	}
}