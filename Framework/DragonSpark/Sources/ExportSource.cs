using System;
using System.Collections.Generic;
using DragonSpark.Application;
using DragonSpark.Extensions;

namespace DragonSpark.Sources
{
	public sealed class ExportSource<T> : ItemSourceBase<T>
	{
		public static ExportSource<T> Default { get; } = new ExportSource<T>();
		
		readonly Func<IExportProvider> source;
		readonly string name;

		public ExportSource( string name = null ) : this( Defaults.DefaultSource, name ) {}

		public ExportSource( Func<IExportProvider> source, string name = null )
		{
			this.source = source;
			this.name = name;
		}

		protected override IEnumerable<T> Yield() => source().GetExports<T>( name ).AsEnumerable();
	}
}