using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Sources
{
	public static class Defaults
	{
		public static Func<IExportProvider> DefaultSource { get; } = Exports.Instance.Get;
	}

	public interface IItemSource<T> : ISource<ImmutableArray<T>>, IEnumerable<T> {}

	public abstract class ItemSourceBase<T> : SourceBase<ImmutableArray<T>>, IItemSource<T>
	{
		public sealed override ImmutableArray<T> Get() => Yield().ToImmutableArray();
		
		protected abstract IEnumerable<T> Yield();

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => Yield().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Yield().GetEnumerator();
	}

	public sealed class ExportSource<T> : ItemSourceBase<T>
	{
		public static ExportSource<T> Instance { get; } = new ExportSource<T>();
		
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

	public class CompositeItemSource<T> : ItemSourceBase<T>
	{
		readonly ImmutableArray<IItemSource<T>> sources;

		public CompositeItemSource( params IItemSource<T>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		protected override IEnumerable<T> Yield() => sources.AsEnumerable().Concat();
	}

	public class ItemSource<T> : ItemSourceBase<T>
	{
		readonly IEnumerable<T> items;

		public ItemSource() : this( Items<T>.Default ) {}

		public ItemSource( params T[] items ) : this( items.AsEnumerable() ) {}

		public ItemSource( IEnumerable<T> items )
		{
			this.items = items;
		}

		protected override IEnumerable<T> Yield() => items;
	}
}