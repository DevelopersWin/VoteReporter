using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonSpark.Composition
{
	public sealed class SingletonExports : ExportSourceBase<SingletonExport>, IEnumerable<SingletonExport>
	{
		readonly IDictionary<Type, SingletonExport> dictionary;
		public SingletonExports( IDictionary<Type, SingletonExport> dictionary ) : base( dictionary.Keys )
		{
			this.dictionary = dictionary;
		}
		public override SingletonExport Get( Type parameter ) => dictionary[ parameter ];
		public IEnumerator<SingletonExport> GetEnumerator() => dictionary.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}