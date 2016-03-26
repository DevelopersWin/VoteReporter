using System.Collections.Generic;
using DragonSpark.Activation;

namespace DragonSpark.Aspects
{
	public abstract class KeyFactory<T> : FactoryBase<IEnumerable<object>, T>
	{
		public T CreateUsing( params object[] parameter ) => Create( parameter );
	}

	public class KeyFactory : KeyFactory<int>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		protected override int CreateItem( IEnumerable<object> parameter )
		{
			var result = 0x2D2816FE;
			foreach ( var o in parameter )
			{
				var next = result * 31;
				var increment = o?.GetHashCode() ?? 0;
				result += next + increment;
			}
			return result;
		}
	}
}