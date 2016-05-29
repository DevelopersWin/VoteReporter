using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public static class Default<T>
	{
		public static Action<T> Empty => t => { };

		public static Func<T, T> Self => t => t;

		public static Func<T, object> Boxed => t => t;

		// [Freeze]
		public static T Item => /*(T)DefaultValueFactory.Instance.Create( typeof(T) )*/default(T);

		// [Freeze]
		public static T[] Items => (T[])/*DefaultValueFactory.Instance.Create( typeof(T[]) )*/Enumerable.Empty<T>() /*Enumerable.Empty<T>().Fixed()*/;
	}

	// [AutoValidation( false )]
	public class DefaultValueFactory<T> : FactoryBase<T>
	{
		readonly Func<Type, object> source;

		public static DefaultValueFactory<T> Instance { get; } = new DefaultValueFactory<T>( DefaultValueFactory.Instance.Create );

		DefaultValueFactory( Func<Type, object> source )
		{
			this.source = source;
		}

		public override T Create() => (T)source( typeof(T) );
	}

	// [AutoValidation( false )]
	public class DefaultValueFactory : FactoryBase<Type, object>
	{
		public static DefaultValueFactory Instance { get; } = new DefaultValueFactory();

		DefaultValueFactory() {}

		[Freeze]
		public override object Create( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? typeof(Enumerable).Adapt().Invoke( nameof(Enumerable.Empty), type.ToItem() ) : GetType().Adapt().Invoke( nameof(Default), new [] { parameter } );
			return result;
		}

		static object Default<T>() => default(T);
	}
}