using System.Collections;
using System.Collections.Immutable;

namespace DragonSpark.Aspects
{
	// [AutoValidation( false )]
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

	/*class AssociatedHash : AttachedPropertyBase<object, Tuple<int>>
	{
		public static AssociatedHash Instance { get; } = new AssociatedHash();

		AssociatedHash() : base( key => new Tuple<int>( key.GetHashCode() ) ) {}
	}*/

	public sealed class KeyFactory //  : KeyFactory<int>
	{
		public static KeyFactory Instance { get; } = new KeyFactory();

		// public string ToString( params object[] items ) => Create( items ).ToString();

		public int CreateUsing( params object[] parameter ) => Create( ImmutableArray.Create( parameter ) );

		public int Create( ImmutableArray<object> parameter )
		{
			var result = 0x2D2816FE;
			for ( var i = 0; i < parameter.Length; i++ )
			{
				var next = result * 31;
				var item = parameter[i];
				var increment = item != null ? GetCode( item ) : 0;
				result += next + increment;
			}
			return result;
		}

		int GetCode( object key )
		{
			var items = key as IList;
			var result = items != null ? CreateFrom( items ) : key.GetHashCode();
			return result;
		}

		int CreateFrom( IEnumerable items )
		{
			var array = ImmutableArray.CreateBuilder<object>();
			foreach ( var item in items )
			{
				array.Add( item );
			}
			var result = Create( array.ToImmutable() );
			return result;
		}

		/*readonly CacheContext context = new CacheContext();

		class CacheContext
		{
			readonly ISet<object> keys = new HashSet<object>();
			readonly IDictionary<object, int> dictionary = new Dictionary<object, int>();

			public int Get( object item )
			{
				if ( !keys.Contains( item ) )
				{
					keys.Add( item );
					dictionary[item] = item.GetHashCode();
				}

				return dictionary[item];
			}
		}*/
	}
}