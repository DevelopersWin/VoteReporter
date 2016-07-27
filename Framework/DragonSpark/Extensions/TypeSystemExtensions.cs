using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
	public static class TypeSystemExtensions
	{
		/*readonly static Func<Assembly, IEnumerable<Type>> CollectionSelector = AssemblyTypes.Public.ToDelegate();

		public static ValueTuple<TAttribute, Type>[] GetAllTypesWith<TAttribute>( this IEnumerable<Assembly> target ) where TAttribute : Attribute
			=> target.SelectMany( CollectionSelector ).WhereDecorated<TAttribute>();*/
		
		public static IEnumerable<Type> Decorated<TAttribute>( this IEnumerable<Type> target ) where TAttribute : Attribute => target.Where( info => info.Has<TAttribute>() );

		public static IEnumerable<Type> AsTypes( this IEnumerable<TypeInfo> target ) => target.Select( info => info.AsType() );

		public static IEnumerable<TypeInfo> AsTypeInfos( this IEnumerable<Type> target ) => target.Select( info => info.GetTypeInfo() );
	}
}