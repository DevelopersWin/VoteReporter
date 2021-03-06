﻿using DragonSpark.Composition;
using DragonSpark.TypeSystem;
using System.Collections.Immutable;

namespace DragonSpark.Application
{
	sealed class DefaultExportProvider : IExportProvider
	{
		public static DefaultExportProvider Default { get; } = new DefaultExportProvider();
		DefaultExportProvider() {}

		public ImmutableArray<T> GetExports<T>( string name = null ) => ApplicationParts.IsAssigned ? SingletonExportSource<T>.Default.Get() : Items<T>.Immutable;
	}
}