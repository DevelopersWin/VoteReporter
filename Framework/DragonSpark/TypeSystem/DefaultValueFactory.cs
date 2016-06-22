using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public static class Delegates
	{
		public static Action Empty { get; } = () => {};

		// public static Func<object, object> Self { get; } = t => t;
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
		public static T[] Default { get; } = (T[])Enumerable.Empty<T>();
	}

	public class DefaultValueFactory<T> : FactoryBase<T>
	{
		readonly Func<Type, object> source;

		public static DefaultValueFactory<T> Instance { get; } = new DefaultValueFactory<T>( DefaultValueFactory.Instance.ToDelegate() );

		DefaultValueFactory( Func<Type, object> source )
		{
			this.source = source;
		}

		public override T Create() => (T)source( typeof(T) );
	}

	// [AutoValidation( false )]
	public class DefaultValueFactory : FactoryWithSpecificationBase<Type, object>
	{
		public static DefaultValueFactory Instance { get; } = new DefaultValueFactory();

		DefaultValueFactory() {}

		[Freeze]
		public override object Create( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? typeof(Enumerable).Adapt().GenericMethods.Invoke( nameof(Enumerable.Empty), type.ToItem() ) : GetType().Adapt().GenericMethods.Invoke( nameof(Default), parameter.ToItem() );
			return result;
		}

		static object Default<T>() => default(T);
	}
}