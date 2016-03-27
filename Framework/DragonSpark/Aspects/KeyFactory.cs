using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DragonSpark.Aspects
{
	public abstract class KeyFactory<T> : FactoryBase<IEnumerable<object>, T>
	{
		public T CreateUsing( params object[] parameter ) => Create( parameter );
	}

	/*public class MemberInfoTransformer : FactoryBase<MemberInfo, int>
	{
		public static MemberInfoTransformer Instance { get; } = new MemberInfoTransformer();

		protected override int CreateItem( MemberInfo parameter ) => parameter is TypeInfo
			? 
			parameter.GetHashCode() : 
			parameter.DeclaringType.GetTypeInfo().GUID.GetHashCode() * 6776 + parameter.ToString().GetHashCode();

		/*static string Build( MemberInfo parameter )
		{
			var builder = new StringBuilder();
			builder.AppendFormat( "{0}+{1}",  );
			var result = builder.ToString();
			return result;
		}#1#
	}*/

	public class KeyFactory : KeyFactory<int>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		protected override int CreateItem( IEnumerable<object> parameter )
		{
			var result = 0x2D2816FE;
			foreach ( var o in parameter )
			{
				var next = result * 31;
				/*var memberInfo = o as MemberInfo;
				var item = memberInfo != null ? MemberInfoTransformer.Instance.Create( memberInfo ) : o;*/
				var increment = o?.GetHashCode() ?? 0;
				result += next + increment;
			}
			return result;
		}
	}
}