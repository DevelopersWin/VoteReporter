using DragonSpark.Application;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Extensions
	{
		readonly static Func<object, Type> CoerceType = TypeCoercer.Default.Get;

		public static Type GetMemberType( this MemberInfo memberInfo ) =>
			( memberInfo as MethodInfo )?.ReturnType ??
			( memberInfo as PropertyInfo )?.PropertyType ??
			( memberInfo as FieldInfo )?.FieldType ??
			( memberInfo as TypeInfo )?.AsType();

		public static IEnumerable<Assembly> Assemblies( this IEnumerable<Type> @this ) => @this.Select( x => x.Assembly() ).Distinct();

		public static IEnumerable<Type> SelectTypes( this IEnumerable<object> @this ) => @this.Select( CoerceType );
		public static ImmutableArray<Type> AsTypes( this IEnumerable<object> @this ) => @this.SelectTypes().ToImmutableArray();

		public static Assembly Assembly( this Type @this ) => @this.GetTypeInfo().Assembly;

		public static ImmutableArray<Type> GetParameterTypes( this MethodBase @this ) => Support.ParameterTypes.Get( @this );

		public static IEnumerable<Type> Decorated<T>( this IEnumerable<Type> target ) where T : Attribute => target.Where( info => info.Has<T>() );

		public static IEnumerable<Type> AsTypes( this IEnumerable<TypeInfo> target ) => target.Select( info => info.AsType() );

		public static ImmutableArray<Type> AsApplicationParts( this IEnumerable<Type> target ) => target.ToImmutableArray().AsApplicationParts();
		public static ImmutableArray<Type> AsApplicationParts( this ImmutableArray<Type> target ) => ApplicationPartsFactory.Default.Get( target ).Types;

		public static IGenericMethodContext<Invoke> GetFactory( this Type @this, string methodName ) => GenericFactories.Default.Get( @this )[methodName];

		sealed class GenericFactories : DecoratedCache<Type, GenericStaticMethodFactories>
		{
			public static GenericFactories Default { get; } = new GenericFactories();
			GenericFactories() {}
		}

		public static IGenericMethodContext<Execute> GetCommand( this Type @this, string methodName ) => GenericCommands.Default.Get( @this )[methodName];

		sealed class GenericCommands : DecoratedCache<Type, GenericStaticMethodCommands>
		{
			public static GenericCommands Default { get; } = new GenericCommands();
			GenericCommands() {}
		}

		public static ImmutableArray<Type> WithNested( this Type @this ) => NestedTypes.Default.Get( @this );

		public static bool IsAssignableFrom( this Type @this, Type other ) => @this.GetTypeInfo().IsAssignableFrom( other );
		public static bool IsAssignableFrom( this TypeInfo @this, Type other ) => TypeAssignableSpecification.Default.Get( @this ).IsSatisfiedBy( other );
		public static bool IsInstanceOfType( this Type @this, object instance ) => @this.IsAssignableFrom( instance.GetType() );
		public static bool ImplementsGeneric( this Type @this, Type other ) => @this.GetTypeInfo().ImplementsGeneric( other );
		public static bool ImplementsGeneric( this TypeInfo @this, Type other ) => GenericImplementationSpecification.Default.Get( @this ).IsSatisfiedBy( other );

		public static Type GetInnerType( this Type @this ) => InnerTypes.Default.Get( @this );
		public static Type GetEnumerableType( this Type @this ) => EnumerableTypes.Default.Get( @this );
		public static ImmutableArray<Type> GetHierarchy( this Type @this ) => TypeHierarchies.Default.Get( @this );
		public static ImmutableArray<Type> GetAllInterfaces( this Type @this ) => AllInterfaces.Default.Get( @this );
		public static ImmutableArray<Type> GetImplementations( this Type @this, Type genericInterface ) => Implementations.Default.Get( @this ).Get( genericInterface );
		public static ImmutableArray<MethodMapping> GetMappedMethods( this Type @this, Type @interface ) => InterfaceMappings.Default.Get( @this ).Get( @interface );

		static class Support
		{
			public static IParameterizedSource<MethodBase, ImmutableArray<Type>> ParameterTypes { get; } = Caches.Create<MethodBase, ImmutableArray<Type>>( method => method.GetParameters().Select( info => info.ParameterType ).ToImmutableArray() );
		}
	}
}
