using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Extensions
{
	/// <summary>
	/// Class that provides extension methods to Collection
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Add a range of items to a collection.
		/// </summary>
		/// <typeparam name="T">Type of objects within the collection.</typeparam>
		/// <param name="collection">The collection to add items to.</param>
		/// <param name="items">The items to add to the collection.</param>
		/// <returns>The collection.</returns>
		/// <exception cref="System.ArgumentNullException">An <see cref="System.ArgumentNullException"/> is thrown if <paramref name="collection"/> or <paramref name="items"/> is <see langword="null"/>.</exception>
		public static ICollection<T> AddRange<T>( this ICollection<T> collection, IEnumerable<T> items)
		{
			var list = collection as List<T>;
			if ( list != null )
			{
				list.AddRange( items );
				return list;
			}

			foreach (var each in items)
			{
				collection.Add(each);
			}

			return collection;
		}

		public static ImmutableArray<T> Purge<T>( this ICollection<T> @this )
		{
			var result = @this.WhereAssigned().ToImmutableArray();
			@this.Clear();
			return result;
		}
	}
}
