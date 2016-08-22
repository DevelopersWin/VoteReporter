using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Extensions
{
	public static class MethodExtensions
	{
		public static T CreateDelegate<T>( this MethodInfo @this ) => @this.CreateDelegate( typeof(T) ).To<T>();

		public static MethodInfo AccountForGenericDefinition( this MethodInfo @this )
		{
			var result = @this.DeclaringType.IsConstructedGenericType ? @this.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().SingleOrDefault( MethodEqualitySpecification.For( @this ) )
				:
				@this;
			return result;
		}

		public static MethodInfo LocateInDerivedType( this MethodInfo @this, Type derivedType )
		{
			/*if ( !@this.DeclaringType.Adapt().IsInstanceOfTypeOrDefinition( derivedType ) )
			{
				throw new InvalidOperationException( $"{derivedType} does not inherit from {@this.DeclaringType}" );
			}*/
			var result = derivedType.GetRuntimeMethods().Introduce( @this, tuple => MethodEqualitySpecification.For( tuple.Item1 )( tuple.Item2 ) ).SingleOrDefault();
			return result;
		}

		public static MethodInfo AccountForClosedDefinition( this MethodInfo @this, Type closedType )=> @this.ContainsGenericParameters ? @this.LocateInDerivedType( closedType ) : @this;
	}

	class MethodEqualitySpecification : SpecificationWithContextBase<MethodInfo>
	{
		public static Func<MethodInfo, Func<MethodInfo, bool>> For { get; } = new Cache<MethodInfo, Func<MethodInfo, bool>>( info => new MethodEqualitySpecification( info ).ToSpecificationDelegate() ).ToDelegate();

		readonly Func<Type, Type> map;

		MethodEqualitySpecification( MethodInfo context ) : base( context )
		{
			map = Map;
		}

		public override bool IsSatisfiedBy( MethodInfo parameter ) =>
			parameter.Name == Context.Name && Map( parameter.ReturnType ) == Context.ReturnType && parameter.GetParameterTypes().Select( map ).SequenceEqual( Context.GetParameterTypes() );

		Type Map( Type type )
		{
			var result = type.IsGenericParameter ? Context.DeclaringType.GenericTypeArguments[type.GenericParameterPosition] : type.GetTypeInfo().ContainsGenericParameters ? 
				type.GetGenericTypeDefinition().MakeGenericType( type.GenericTypeArguments.Select( map ).ToArray() ) : type;
			return result;
		}
	}
}