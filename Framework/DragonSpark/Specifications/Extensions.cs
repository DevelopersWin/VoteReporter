using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Specifications
{
	public static class Extensions
	{
		public static ISpecification<TFrom> With<TFrom, TTo>( this ISpecification<TTo> @this, ICoercer<TFrom, TTo> coercer ) => With( @this.ToSpecificationDelegate(), coercer.ToDelegate() );
		public static ISpecification<TFrom> With<TFrom, TTo>( this Func<TTo, bool> @this, Func<TFrom, TTo> coerce ) =>
			new CoercedSpecification<TFrom, TTo>( coerce, @this );

		// public static bool Accepts<T>( this object @this, T parameter ) => @this.ToSpecification<T>().IsSatisfiedBy( parameter );

		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => Inversed<T>.Default.Get( @this );
		sealed class Inversed<T> : Cache<ISpecification<T>, ISpecification<T>>
		{
			public static Inversed<T> Default { get; } = new Inversed<T>();
			Inversed() : base( specification => new InverseSpecification<T>( specification ) ) {}
		}

		public static ISpecification<T> Or<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) => new AnySpecification<T>( @this.Append( others ).Fixed() );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, params ISpecification<T>[] others ) => new AllSpecification<T>( @this.Append( others ).Fixed() );

		public static ISpecification<TDestination> Project<TDestination, TOrigin>( this ISpecification<TOrigin> @this, Func<TDestination, TOrigin> projection ) => new ProjectedSpecification<TOrigin, TDestination>( @this.IsSatisfiedBy, projection );

		// public static ISpecification<TTo> Cast<TFrom, TTo>( this ISpecification<TFrom> @this ) where TFrom : TTo => new CastingSpecification<TFrom, TTo>( @this );

		public static ISpecification<T> Cast<T>( this ISpecification @this ) => @this as ISpecification<T> ?? Casts<T>.Default.Get( @this );
		sealed class Casts<T> : Cache<ISpecification, ISpecification<T>>
		{
			public static Casts<T> Default { get; } = new Casts<T>();
			Casts() : base( specification => new CastingSpecification<T>( specification ) ) {}
		}

		public static ISpecification<object> Fixed<T>( this ISpecification<T> @this, T parameter ) => new SuppliedDelegatedSpecification<T>( @this, parameter )/*.Cast<T>()*/;
		public static ISpecification<object> Fixed<T>( this ISpecification<T> @this, Func<T> parameter ) => new SuppliedDelegatedSpecification<T>( @this, parameter )/*.Cast<T>()*/;

		public static Func<object, bool> ToSpecificationDelegate( this ISpecification @this ) => Delegates.Default.Get( @this );
		sealed class Delegates : Cache<ISpecification, Func<object, bool>>
		{
			public static Delegates Default { get; } = new Delegates();
			Delegates() : base( specification => specification.IsSatisfiedBy ) {}
		}

		public static Func<T, bool> ToSpecificationDelegate<T>( this ISpecification<T> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<ISpecification<T>, Func<T, bool>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( specification => specification.IsSatisfiedBy ) {}
		}

		public static ISpecification<T> ToCachedSpecification<T>( this ISpecification<T> @this ) => CachedSpecifications<T>.Default.Get( @this );
		sealed class CachedSpecifications<T> : Cache<ISpecification<T>, ISpecification<T>>
		{
			public static CachedSpecifications<T> Default { get; } = new CachedSpecifications<T>();
			CachedSpecifications() : base( specification => new DelegatedSpecification<T>( specification.ToSpecificationDelegate().Cache() ) ) {}
		}

		/*public static ISpecification<T> ToSpecification<T>( this object @this ) => Specifications<T>.Default.Get( @this );
		sealed class Specifications<T> : Cache<ISpecification<T>>
		{
			public static Specifications<T> Default { get; } = new Specifications<T>();
			Specifications() : base( SpecificationLocator<T>.Default.Get ) {}
		}*/

		/*public static ISpecification<TParameter> ToSpecification<TParameter, TResult>( this IValidatedParameterizedSource<TParameter, TResult> @this ) => FactorySpecifications<TParameter, TResult>.Default.Get( @this );
		sealed class FactorySpecifications<TParameter, TResult> : Cache<IValidatedParameterizedSource<TParameter, TResult>, ISpecification<TParameter>>
		{
			public static FactorySpecifications<TParameter, TResult> Default { get; } = new FactorySpecifications<TParameter, TResult>();
			FactorySpecifications() : base( factory => new DelegatedSpecification<TParameter>( factory.IsSatisfiedBy ) ) {}
		}*/
	}
}