using System;
using System.Collections.Immutable;

namespace DragonSpark.Setup.Registration
{
	public struct ConventionTypeContainer
	{
		public ConventionTypeContainer( ImmutableArray<Type> candidates )
		{
			Candidates = candidates;
		}

		public ImmutableArray<Type> Candidates { get; }
	}
}