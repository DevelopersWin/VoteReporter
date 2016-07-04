using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using Microsoft.Practices.Unity.Utility;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Activator = System.Activator;

namespace DragonSpark.TypeSystem
{
	public static class Delegates
	{
		public static Action Empty { get; } = () => {};

		public static Func<TParameter, TResult> From<TParameter, TResult>() => Cache<TParameter, TResult>.Instance.Get( typeof(TResult).GetConstructor( typeof(TParameter) ) );

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
			}*/
		}
	}

	public static class SpecialValues
	{
		public static object Null { get; } = new object();

		public static T DefaultOrEmpty<T>() => DefaultValueFactory<T>.Instance();

		public static object DefaultOrEmpty( Type type ) => DefaultValueFactory.Instance( type );
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
			Immutable = ImmutableArray<T>.Empty;
		}

		public static T[] Default { get; }

		public static ImmutableArray<T> Immutable { get; }
	}

	class DefaultValueFactory<T> : FactoryBase<T>
	{
		readonly Func<Type, object> source;

		public static Func<T> Instance { get; } = new DefaultValueFactory<T>( DefaultValueFactory.Instance ).ToDelegate();

		DefaultValueFactory( Func<Type, object> source )
		{
			this.source = source;
		}

		public override T Create() => (T)source( typeof(T) );
	}

	class DefaultValueFactory : FactoryBase<Type, object>
	{
		readonly static IGenericMethodContext Method = typeof(Enumerable).Adapt().GenericMethods[nameof(Enumerable.Empty)];

		public static Func<Type, object> Instance { get; } = new DefaultValueFactory().ToDelegate();

		DefaultValueFactory() {}

		[Freeze]
		public override object Create( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? Method.Make( type.ToItem() ).StaticInvoke<Array>() : parameter.GetTypeInfo().IsValueType ? Activator.CreateInstance( parameter ) : null;
			return result;
		}
	}
}