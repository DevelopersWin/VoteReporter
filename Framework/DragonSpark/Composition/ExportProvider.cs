using System.Collections.Immutable;
using System.Composition;
using DragonSpark.Application;
using DragonSpark.Extensions;

namespace DragonSpark.Composition
{
	public sealed class ExportProvider : IExportProvider
	{
		readonly CompositionContext context;
		public ExportProvider( CompositionContext context )
		{
			this.context = context;
		}

		public ImmutableArray<T> GetExports<T>( string name = null ) => context.GetExports<T>( name ).WhereAssigned().Prioritize().ToImmutableArray();
	}
}