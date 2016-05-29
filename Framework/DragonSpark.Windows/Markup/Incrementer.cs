using DragonSpark.Runtime.Properties;

namespace DragonSpark.Windows.Markup
{
	class Incrementer : IIncrementer
	{
		readonly AttachedProperty<int> count = new AttachedProperty<int>();

		public int Next( object context )
		{
			var result = count.Get( context ) + 1;
			count.Set( context, result );
			return result;
		}
	}
}