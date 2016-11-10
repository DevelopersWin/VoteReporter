using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Sources.Scopes;
using System;
using System.Reflection;

namespace DragonSpark.Specifications
{
	public static class Extensions
	{
		public static ISpecification<TFrom> Coerce<TFrom, TTo>( this ISpecification<TTo> @this, IParameterizedSource<TFrom, TTo> coercer ) => Coerce( @this, coercer.ToDelegate() );
		public static ISpecification<TFrom> Coerce<TFrom, TTo>( this ISpecification<TTo> @this, Func<TFrom, TTo> coercer ) => Coerce( @this.ToDelegate(), coercer );
		public static ISpecification<TFrom> Coerce<TFrom, TTo>( this Func<TTo, bool> @this, Func<TFrom, TTo> coerce ) =>
			new CoercedSpecification<TFrom, TTo>( coerce, @this );

		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => Inversed<T>.Default.Get( @this );
		sealed class Inversed<T> : Cache<ISpecification<T>, ISpecification<T>>
		{
			public static Inversed<T> Default { get; } = new Inversed<T>();
			Inversed() : base( specification => new InverseSpecification<T>( specification ) ) {}
		}

		public static ISpecification<T> Or<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) => new AnySpecification<T>( @this.Append( others ).Fixed() );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) => new AllSpecification<T>( @this.Append( others ).Fixed() );

		// public static ISpecification<TDestination> Project<TDestination, TOrigin>( this ISpecification<TOrigin> @this, Func<TDestination, TOrigin> projection ) => new ProjectedSpecification<TOrigin, TDestination>( @this.IsSatisfiedBy, projection );

		public static ISpecification<object> Fixed<T>( this ISpecification<T> @this, T parameter ) => new SuppliedDelegatedSpecification<T>( @this, parameter );
		public static ISpecification<object> Fixed<T>( this ISpecification<T> @this, Func<T> parameter ) => new SuppliedDelegatedSpecification<T>( @this, parameter );

		public static bool IsSatisfiedBy( this ISpecification<TypeInfo> @this, Type parameter ) => @this.ToDelegate().Get( parameter );
		public static bool IsSatisfiedBy( this Func<TypeInfo, bool> @this, Type parameter ) => @this( parameter.GetTypeInfo() );
		public static bool IsSatisfiedBy( this ISpecification<Type> @this, TypeInfo parameter ) => @this.ToDelegate().Get( parameter );
		public static bool IsSatisfiedBy( this Func<Type, bool> @this, TypeInfo parameter ) => @this( parameter.AsType() );

		public static Func<T, bool> ToDelegate<T>( this ISpecification<T> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<ISpecification<T>, Func<T, bool>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( specification => specification.IsSatisfiedBy ) {}
		}

		public static ISpecification<T> ToCachedSpecification<T>( this ISpecification<T> @this ) => CachedSpecifications<T>.Default.Get( @this );
		sealed class CachedSpecifications<T> : Cache<ISpecification<T>, ISpecification<T>>
		{
			public static CachedSpecifications<T> Default { get; } = new CachedSpecifications<T>();
			CachedSpecifications() : base( specification => new DelegatedSpecification<T>( specification.ToDelegate().ToSingleton() ) ) {}
		}
	}
}