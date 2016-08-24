﻿using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Io;
using System.Collections.Immutable;

namespace DragonSpark.Windows.TypeSystem
{
	public sealed class AssemblyLocator : QueryableResourceLocator
	{
		public static IParameterizedSource<string, ImmutableArray<string>> Default { get; } = new AssemblyLocator().ToEqualityCache();
		AssemblyLocator() : base( IsAssemblyFileSpecification.Default.IsSatisfiedBy ) {}
	}
}
