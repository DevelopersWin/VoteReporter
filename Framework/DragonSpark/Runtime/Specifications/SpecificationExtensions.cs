using DragonSpark.Extensions;
using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification<T>( @this );

		// public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );

		public static ISpecification<T> ToAny<T>( this IEnumerable<ISpecification> @this ) => new AnySpecification( @this.Fixed() ).Box<T>();

		public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );

		public static ISpecification<T> Box<T>( this ISpecification @this ) => @this.Box<T>( t => t );

		public static ISpecification<T> Box<T>( this ISpecification @this, Func<T, object> projection ) => new BoxedSpecification<T>( @this, projection );
	}
}