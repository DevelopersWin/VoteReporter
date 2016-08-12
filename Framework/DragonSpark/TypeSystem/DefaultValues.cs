using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources.Parameterized.Caching;
using Activator = System.Activator;

namespace DragonSpark.TypeSystem
{
	public static class Delegates
	{
		public static Action Empty { get; } = () => {};

		/*public static Func<TParameter, TResult> From<TParameter, TResult>() => Cache<TParameter, TResult>.Instance.Get( typeof(TResult).GetConstructor( typeof(TParameter) ) );

		public static TResult From<TParameter, TResult>( TParameter parameter ) => From<TParameter, TResult>()( parameter );

		class Cache<TParameter, TResult> : Runtime.Properties.Cache<ConstructorInfo, Func<TParameter, TResult>>
		{
			public static Cache<TParameter, TResult> Instance { get; } = new Cache<TParameter, TResult>();
			Cache() : base( Factory.Instance.ToDelegate() ) {}

			class Factory : CompiledDelegateFactoryBase<ConstructorInfo, TParameter, Func<TParameter, TResult>>
			{
				public static Factory Instance { get; } = new Factory();

				protected override Expression CreateBody( ConstructorInfo parameter, ParameterExpression definition ) => Expression.New( parameter, definition );
			}

			/*class ConstructorCache : Runtime.Properties.Cache<Type, ConstructorInfo>
			{
				ConstructorCache() : base( type => typeof(TResult).GetConstructor( type ) ) {}
			}#1#
		}*/
	}

	public static class SpecialValues
	{
		public static object Null { get; } = new object();

		public static T DefaultOrEmpty<T>() => Default<T>.Instance;

		public static object DefaultOrEmpty( Type type ) => DefaultValues.Instance.Get( type );
	}

	public static class Delegates<T>
	{
		public static Action<T> Empty { get; } = t => {};

		public static Func<T, T> Self { get; } = t => t;

		public static Func<T, object> Object { get; } = t => t;

		public static Func<T> Default { get; } = () => default(T);
	}

	public static class Items<T>
	{
		static Items()
		{
			Default = (T[])Enumerable.Empty<T>();
			Immutable = Default.ToImmutableArray();
			List = Default.ToImmutableList();
		}

		public static T[] Default { get; }

		public static ImmutableArray<T> Immutable { get; }

		public static IList<T> List { get; }
	}

	static class Default<T>
	{
		public static T Instance { get; } = (T)DefaultValues.Instance.Get( typeof(T) );
	}

	class DefaultValues : Cache<Type, object>
	{
		readonly static IGenericMethodContext<Invoke> Method = typeof(Enumerable).Adapt().GenericFactoryMethods[nameof(Enumerable.Empty)];

		public static ICache<Type, object> Instance { get; } = new DefaultValues();
		DefaultValues() : base( Create ) {}

		static object Create( Type parameter ) => parameter.GetTypeInfo().IsValueType ? Activator.CreateInstance( parameter ) : Empty( parameter );

		static object Empty( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? Method.Make( type.ToItem() ).Invoke<Array>() : null;
			return result;
		}
	}
}