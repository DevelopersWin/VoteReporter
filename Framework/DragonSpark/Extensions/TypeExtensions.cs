using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
	public static class ImmutableArrayExtensions
	{
		public static IEnumerable<T> Concat<T>( this ImmutableArray<T> first, IEnumerable<T> second ) => first.ToArray().Concat( second );

		public static IEnumerable<T> Concat<T>( this IEnumerable<ImmutableArray<T>> sources ) => sources.Select( array => array.ToArray() ).Concat();
	}

	public static class TypeExtensions
	{
		public static Type GetMemberType(this MemberInfo memberInfo)
		{
		  if (memberInfo is MethodInfo)
			return ((MethodInfo) memberInfo).ReturnType;
		  if (memberInfo is PropertyInfo)
			return ((PropertyInfo) memberInfo).PropertyType;
		  if (memberInfo is FieldInfo)
			return ((FieldInfo) memberInfo).FieldType;
		  return null;
		}

		public static ImmutableArray<Assembly> Assemblies( this ImmutableArray<Type> @this ) => @this.Select( x => x.Assembly() ).Distinct().ToImmutableArray();

		public static TypeAdapter Adapt( this Type @this ) => TypeAdapterCache.Default.Get( @this );

		public static TypeAdapter Adapt( this object @this ) => @this.GetType().Adapt();

		public static TypeAdapter Adapt( this TypeInfo @this ) => Adapt( @this.AsType() );

		public static Assembly Assembly( this Type @this ) => Adapt( @this ).Assembly;

		readonly static TypeInfo Structural = typeof(IStructuralEquatable).GetTypeInfo();

		public static bool IsStructural( this Type @this ) => Structural.IsAssignableFrom( @this.GetTypeInfo() );
		
		public static bool IsAssignableFrom( this ImmutableArray<TypeAdapter> @this, Type type )
		{
			foreach ( var adapter in @this )
			{
				if ( adapter.IsAssignableFrom( type ) )
				{
					return true;
				}
			}
			return false;
		}

		readonly static ICache<MethodBase, Type[]> Parameters = new Cache<MethodBase, Type[]>( method => method.GetParameters().Select( info => info.ParameterType ).ToArray() );
		public static Type[] GetParameterTypes( this MethodBase @this ) => Parameters.Get( @this );
	}

	public class TypeAdapterCache : Cache<Type, TypeAdapter>
	{
		public static TypeAdapterCache Default { get; } = new TypeAdapterCache();

		TypeAdapterCache() : base( t => new TypeAdapter( t ) ) {}
	}
}