using DragonSpark.Activation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Aspects
{
	[AutoValidation( false )]
	public abstract class KeyFactory<T> : FactoryBase<IEnumerable<object>, T>
	{
		public T CreateUsing( params object[] parameter ) => Create( parameter );
	}

	/*public class MemberInfoTransformer : Factory<MemberInfo, int>
	{
		public static MemberInfoTransformer Instance { get; } = new MemberInfoTransformer();

		public MemberInfoTransformer() : base( IsTypeSpecification<MemberInfo>.Instance ) {}

		protected override int CreateItem( MemberInfo parameter ) => parameter is TypeInfo
			? 
			parameter.GetHashCode() : 
			parameter.DeclaringType.GetTypeInfo().GUID.GetHashCode() * 6776 + parameter.ToString().GetHashCode();
	}

	public class ParameterInfoTransformer : Factory<ParameterInfo, int>
	{
		public static ParameterInfoTransformer Instance { get; } = new ParameterInfoTransformer();

		public ParameterInfoTransformer() : base( IsTypeSpecification<ParameterInfo>.Instance ) {}

		protected override int CreateItem( ParameterInfo parameter ) => 
			parameter.Member.DeclaringType.GetTypeInfo().GUID.GetHashCode() * 6776 + parameter.ToString().GetHashCode();
	}*/

	public class KeyFactory : KeyFactory<int>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		public override int Create( IEnumerable<object> parameter )
		{
			var result = 0x2D2816FE;
			foreach ( var o in parameter )
			{
				var next = result * 31;
				var increment = GetCode( o );
				result += next + increment;
			}
			return result;
		}

		int GetCode( object o )
		{
			var text = o as string;
			if ( text != null )
			{
				return text.GetHashCode();
			}

			var items = o as IEnumerable;
			if ( items != null )
			{
				return Create( items.Cast<object>() );
			}

			/*if ( o is MemberInfo )
			{
				return MemberInfoTransformer.Instance.Create( o as MemberInfo );
			}

			if ( o is ParameterInfo )
			{
				return ParameterInfoTransformer.Instance.Create( o as ParameterInfo );
			}*/
			
			return o?.GetHashCode() ?? 0;
		}
	}
}