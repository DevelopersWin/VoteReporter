using System;
using System.Collections.Generic;
using DragonSpark.Commands;

namespace DragonSpark.Runtime.Assignments
{
	public static class CollectionActions
	{
		public static Value<CollectionAction> Default { get; } = new Value<CollectionAction>( CollectionAction.Add, CollectionAction.Remove );

		public static IDisposable Assignment<T>( this ICollection<T> @this, T item ) => new CollectionAssignment<T>( @this, item ).AsExecuted();
	}
}