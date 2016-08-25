using DragonSpark.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Sources
{
	public sealed class SourceCollection : SourceCollection<object>
	{
		public SourceCollection( params ISource[] items ) : base( items ) {}
	}

	public class SourceCollection<T> : CollectionBase<ISource>, ICollection<T>
	{
		public SourceCollection( params ISource[] items ) : base( items.AsEnumerable() ) {}

		public void Add( T item ) => Add( item.Sourced() );

		public bool Contains( T item ) => Values().Contains( item );

		public void CopyTo( T[] array, int arrayIndex ) => Values().ToArray().CopyTo( array, arrayIndex );

		public new IEnumerator<T> GetEnumerator() => Values().GetEnumerator();

		IEnumerable<T> Values() => Source.Get<T>();

		public bool Remove( T item )
		{
			foreach ( var source in Source )
			{
				if ( Equals( source.Get(), item ) )
				{
					return Source.Remove( source );
				}
			}
			return false;
		}
	}
}