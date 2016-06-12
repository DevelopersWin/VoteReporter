using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public static class Invocation
	{
		public static Delegate GetCurrent() => AmbientStack.GetCurrentItem<Delegate>();

		public static T Create<T>( T @delegate ) => Invocation<T>.Default.Get( @delegate as Delegate );
	}

	public class Invocation<T> : AttachedProperty<Delegate, T>
	{
		readonly static MethodInfo MethodInfo = typeof(Invocation<T>).GetTypeInfo().GetDeclaredMethod( nameof(Invoke) );

		public static Invocation<T> Default { get; } = new Invocation<T>();

		Invocation() : base( new Func<Delegate, T>( Create ) ) {}

		static T Create( Delegate inner )
		{
			var parameters = Parameters.Default.Get( inner.GetMethodInfo() );
			var expressions = ImmutableArray.Create<Expression>( Expression.Constant( inner ), Expression.NewArrayInit( typeof(object), parameters ) ).ToArray();
			var call = Expression.Call( null, MethodInfo, expressions );
			var type = inner.GetMethodInfo().ReturnType;
			var convert = type != typeof(void) ? (Expression)Expression.Convert( call, type ) : call;
			var result = Expression.Lambda<T>( convert, parameters ).Compile();
			return result;
		}

		static object Invoke( Delegate target, object[] parameters )
		{
			using ( new AmbientStack<Delegate>.Assignment( target ) )
			{
				var result = target.DynamicInvoke( parameters );
				return result;
			}
		}
	}

	class Parameters : AttachedProperty<MethodBase, ParameterExpression[]>
	{
		public static Parameters Default { get; } = new Parameters();

		Parameters() : base( method => method.GetParameters().ToImmutableArray().Select( info => Expression.Parameter( info.ParameterType, info.Name ) ).ToArray() ) {}
	}
}
