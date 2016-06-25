using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Extensions
{
	public static class MethodExtensions
	{
		public static MethodInfo FromGenericDefinition( this MethodInfo @this )
		{
			var result = @this.DeclaringType.IsConstructedGenericType ? @this.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().SingleOrDefault( MethodEqualitySpecification.Default.Get( @this ).ToDelegate() )
				:
				@this;
			return result;
		}
	}

	class MethodEqualitySpecification : SpecificationWithContextBase<MethodInfo>
	{
		public static ICache<MethodInfo, ISpecification<MethodInfo>> Default { get; } = new Cache<MethodInfo, ISpecification<MethodInfo>>( info => new MethodEqualitySpecification( info ) );

		readonly Func<Type, Type> @select;

		MethodEqualitySpecification( MethodInfo context ) : base( context )
		{
			@select = Map;
		}

		public override bool IsSatisfiedBy( MethodInfo parameter )
		{
			var type = Map( parameter.ReturnType );
			var isSatisfiedBy = parameter.Name == Context.Name && type == Context.ReturnType && parameter.GetParameterTypes().Select( @select ).SequenceEqual( Context.GetParameterTypes() );
			return isSatisfiedBy;
		}

		Type Map( Type type )
		{
			var result = type.IsGenericParameter ? Context.DeclaringType.GenericTypeArguments[type.GenericParameterPosition] : type.GetTypeInfo().ContainsGenericParameters ? 
				type.GetGenericTypeDefinition().MakeGenericType( type.GenericTypeArguments.Select( @select ).ToArray() ) : type;
			return result;
		}
	}
}