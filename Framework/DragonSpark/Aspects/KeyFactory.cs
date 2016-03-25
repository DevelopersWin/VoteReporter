using DragonSpark.Activation.FactoryModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DragonSpark.Aspects
{
	public abstract class KeyFactory<T> : FactoryBase<IEnumerable<object>, T>
	{
		public T CreateUsing( params object[] parameter ) => Create( parameter );
	}

	public class HashCodeKeyFactory : KeyFactory<int>
	{
		public static HashCodeKeyFactory Instance { get; } = new HashCodeKeyFactory();

		protected override int CreateItem( IEnumerable<object> parameter ) => parameter.Aggregate( 0x2D2816FE, ( current, item ) => current * 31 + ( item?.GetHashCode() ?? 0 ) );
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

	/*public class KeyFactory : KeyFactory<string>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		protected override string CreateItem( IEnumerable<object> parameter ) => parameter.Aggregate( string.Empty, ( current, item ) => string.Concat( current, "_", item.GetHashCode() ) );
	}

	public class JoinFactory : KeyFactory<string>
	{
		public static JoinFactory Instance { get; } = new JoinFactory();

		protected override string CreateItem( IEnumerable<object> parameter ) => string.Join( "_", parameter.Select( o => o.GetHashCode() ) );
	}

	class Builder : KeyFactory<string>
	{
		public static Builder Instance { get; } = new Builder();

		protected override string CreateItem( IEnumerable<object> parameter )
		{
			var builder = new StringBuilder();
			foreach ( var o in parameter )
			{
				builder.Append( o.GetHashCode() );
				builder.Append( "_" );
			}
			var result = builder.ToString();
			return result;
		}
	}*/
}