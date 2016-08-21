using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.Runtime;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class EqualityReference<T> : TransformerBase<T> where T : class
	{
		public static EqualityReference<T> Default { get; } = new EqualityReference<T>();
		EqualityReference() {}

		readonly WeakList<T> list = new WeakList<T>();

		T GetOrAdd( T item )
		{
			lock ( list )
			{
				var current = list.Introduce( item, tuple => Equals( tuple.Item1, tuple.Item2 ) ).SingleOrDefault();
				if ( current == null )
				{
					list.Add( item );
					return item;
				}
				return current;
			}
		}

		public override T Get( T parameter ) => GetOrAdd( parameter );
	}
}