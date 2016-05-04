using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		// public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );


		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification( @this ).Cast<T>();
		

		// public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification<T> Or<T>( this ISpecification<T> @this, ISpecification other ) => new AnySpecification( @this, other ).Cast<T>();

		// public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, ISpecification other ) => new AllSpecification( @this, other ).Cast<T>();

		public static ISpecification<T> Cast<T>( this ISpecification @this ) => @this.Cast( Default<T>.Boxed );

		public static ISpecification<T> Cast<T>( this ISpecification @this, Func<T, object> projection ) => new DecoratedSpecification<T>( @this, projection );

		// public static ISpecification<T> Cast<T>( this ISpecification<T> @this ) => @this.Cast( Default<T>.Boxed );
	}
}