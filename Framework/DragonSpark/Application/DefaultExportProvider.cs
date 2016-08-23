using System.Collections.Immutable;
using DragonSpark.TypeSystem;

namespace DragonSpark.Application
{
	class DefaultExportProvider : IExportProvider
	{
		public static DefaultExportProvider Default { get; } = new DefaultExportProvider();
		DefaultExportProvider() {}

		public ImmutableArray<T> GetExports<T>( string name = null ) => Items<T>.Immutable;
	}
}