using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
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

		// public static Type Initialized( this Type @this ) => TypeInitializer.Instance.Create( @this );

		public static Assembly[] Assemblies( [Required] this IEnumerable<Type> @this ) => @this.Select( x => x.Assembly() ).Distinct().ToArray();

		public static TypeAdapter Adapt( [Required]this Type @this ) => TypeAdapterProperty.Instance.Get( @this );

		public static TypeAdapter Adapt( this object @this ) => @this.GetType().Adapt();

		public static TypeAdapter Adapt( [Required]this TypeInfo @this ) => Adapt( @this.AsType() );

		public static Assembly Assembly( [Required]this Type @this ) => Adapt( @this ).Assembly;
	}

	public class TypeAdapterProperty : AttachedPropertyBase<Type, TypeAdapter>
	{
		public static TypeAdapterProperty Instance { get; } = new TypeAdapterProperty();

		public TypeAdapterProperty() : base( t => new TypeAdapter( t ) ) {}
	}
}