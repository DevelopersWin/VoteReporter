using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class ConventionTypes : CompositeFactory<Type, Type>
	{
		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterizedScope<Type, Type>( new ConventionTypes().With( Defaults.ConventionCandidate ).ToSourceDelegate().Fix().Global() );
		ConventionTypes() : base( MappedConventionLocator.Default, ConventionLocator.Default ) {}
	}
}