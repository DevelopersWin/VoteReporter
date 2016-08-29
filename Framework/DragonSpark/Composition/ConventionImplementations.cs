using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class ConventionImplementations : CompositeFactory<Type, Type>
	{
		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterizedScope<Type, Type>( new ConventionImplementations().With( Defaults.ConventionCandidate ).ToSourceDelegate().Fix().Global() );
		ConventionImplementations() : base( MappedConventionLocator.Default, ConventionImplementationLocator.Default ) {}
	}
}