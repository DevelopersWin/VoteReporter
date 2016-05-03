using DragonSpark.TypeSystem;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );

		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => InverseAndProject<T>( @this );

		public static ISpecification<T> Inverse<T>( this ISpecification @this ) => InverseAndProject<T>( @this );
		static ISpecification<T> InverseAndProject<T>( ISpecification @this ) => new InverseSpecification( @this ).Box<T>();

		// public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );

		// public static ISpecification<T> ToAny<T>( this IEnumerable<ISpecification> @this ) => new AnySpecification( @this.Fixed() ).Box<T>();

		public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );


		public static ISpecification<T> Box<T>( this ISpecification @this ) => @this.Box( Default<T>.Boxed );

		public static ISpecification<T> Box<T>( this ISpecification @this, Func<T, object> projection ) => new DecoratedSpecification<T>( @this, projection );

		// public static ISpecification<T> Box<T>( this ISpecification<T> @this ) => @this.Box( Default<T>.Boxed );
	}
}