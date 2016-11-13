using System.Collections.Generic;

namespace DragonSpark.Runtime.Assignments
{
	public sealed class CollectionAssign<T> : IAssign<T, CollectionAction>
	{
		readonly ICollection<T> collection;

		public CollectionAssign( ICollection<T> collection )
		{
			this.collection = collection;
		}

		public void Assign( T first, CollectionAction second )
		{
			switch ( second )
			{
				case CollectionAction.Add:
					collection.Add( first );
					break;
				case CollectionAction.Remove:
					collection.Remove( first );
					break;
			}
		}
	}
}