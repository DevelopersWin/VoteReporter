using System.Collections.Generic;
using System.Windows.Markup;

namespace DragonSpark.Runtime
{
	public class DeclarativeCollection : DeclarativeCollection<object>
	{
		public DeclarativeCollection() {}
		public DeclarativeCollection( IEnumerable<object> collection ) : base( collection ) {}
		public DeclarativeCollection( IList<object> items ) : base( items ) {}
	}

	[Ambient]
	public class DeclarativeCollection<T> : CollectionBase<T>
	{
		public DeclarativeCollection() {}
		public DeclarativeCollection( IEnumerable<T> collection ) : base( collection ) {}
		public DeclarativeCollection( IList<T> items ) : base( items ) {}
	}
}