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

	public class KeyFactory : KeyFactory<string>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		protected override string CreateItem( IEnumerable<object> parameter ) => parameter.Aggregate( string.Empty, ( current, item ) => string.Concat( current, "_", item ) );
	}

	public class JoinFactory : KeyFactory<string>
	{
		public static JoinFactory Instance { get; } = new JoinFactory();

		protected override string CreateItem( IEnumerable<object> parameter ) => string.Join( "_", parameter.Select( o => o.ToString() ) );
	}

	class Builder : KeyFactory<string>
	{
		public static Builder Instance { get; } = new Builder();

		protected override string CreateItem( IEnumerable<object> parameter )
		{
			var builder = new StringBuilder();
			foreach ( var o in parameter )
			{
				builder.Append( o );
				builder.Append( "_" );
			}
			var result = builder.ToString();
			return result;
		}
	}
}