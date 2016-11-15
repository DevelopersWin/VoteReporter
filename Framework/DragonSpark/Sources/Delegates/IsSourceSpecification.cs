using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Delegates
{
	public sealed class IsSourceSpecification : CompositeAssignableSpecification
	{
		public static ISpecification<Type> Default { get; } = new IsSourceSpecification().ToCachedSpecification<Type>();
		IsSourceSpecification() : base( typeof(ISource<>), typeof(ISourceAware) ) {}
	}
}