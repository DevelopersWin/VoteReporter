using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Windows.Markup
{
	class Incrementer : IIncrementer
	{
		readonly SourceCache<int> count = new SourceCache<int>();

		public int Next( object context )
		{
			var result = count.Get( context ) + 1;
			count.Set( context, result );
			return result;
		}
	}
}