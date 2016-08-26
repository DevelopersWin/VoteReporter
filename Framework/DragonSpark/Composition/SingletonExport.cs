using System;
using System.Collections.Immutable;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public struct SingletonExport
	{
		public SingletonExport( ImmutableArray<CompositionContract> contracts, Func<object> factory )
		{
			Contracts = contracts;
			Factory = factory;
		}

		public ImmutableArray<CompositionContract> Contracts { get; }
		public Func<object> Factory { get; }
	}
}