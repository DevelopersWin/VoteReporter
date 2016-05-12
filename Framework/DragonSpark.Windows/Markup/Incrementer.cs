using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Windows.Markup
{
	class Incrementer : IIncrementer
	{
		public int Next( object context )
		{
			var count = new Count( context );
			var result = count.Value + 1;
			count.Assign( result );
			return result;
		}

		class Count : AssociatedStore<int>
		{
			public Count( object source ) : base( source, typeof(Count) ) {}
		}
	}
}