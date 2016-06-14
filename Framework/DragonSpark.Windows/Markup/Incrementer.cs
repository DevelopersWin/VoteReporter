using DragonSpark.Runtime.Properties;

namespace DragonSpark.Windows.Markup
{
	class Incrementer : IIncrementer
	{
		readonly StoreCache<int> count = new StoreCache<int>();

		public int Next( object context )
		{
			var result = count.Get( context ) + 1;
			count.Set( context, result );
			return result;
		}
	}
}