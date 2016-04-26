using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
	public static class AssemblyLocatorExtensions
	{
		public static Tuple<TAttribute, TypeInfo>[] GetAllTypesWith<TAttribute>( this IEnumerable<Assembly> target ) where TAttribute : Attribute
			=> target.SelectMany( assembly => assembly.DefinedTypes ).WhereDecorated<TAttribute>();
		
		public static Tuple<TAttribute, TypeInfo>[] WhereDecorated<TAttribute>( this IEnumerable<TypeInfo> target ) where TAttribute : Attribute
			=> target.Where( info => info.Has<TAttribute>() ).Select( info => new Tuple<TAttribute, TypeInfo>( info.GetAttribute<TAttribute>(), info ) ).ToArray();

		public static IEnumerable<Type> AsTypes( this IEnumerable<TypeInfo> target ) => target.Select( info => info.AsType() );

		public static IEnumerable<TypeInfo> AsTypeInfos( this IEnumerable<Type> target ) => target.Select( info => info.GetTypeInfo() );
	}
}