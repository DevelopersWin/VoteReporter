using System.Collections.Generic;

namespace DragonSpark.Runtime.Assignments
{
	public sealed class CollectionAssignment<T> : Assignment<T, CollectionAction>
	{
		public CollectionAssignment( ICollection<T> collection, T item ) : base( new CollectionAssign<T>( collection ), Assignments.From( item ), CollectionActions.Default ) {}
		
	}
}