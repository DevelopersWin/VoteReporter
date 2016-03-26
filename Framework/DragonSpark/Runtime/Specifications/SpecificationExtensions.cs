using System;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification( @this ).Wrap<T>();

		public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );

		public static ISpecification<T> Any<T>( this IEnumerable<ISpecification<T>> @this ) => new AnySpecification( @this.Cast<ISpecification>().Fixed() ).Wrap<T>();

		public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );

		public static ISpecification<T> Wrap<T>( this ISpecification @this ) => @this.Wrap<T>( t => t );

		public static ISpecification<T> Wrap<T>( this ISpecification @this, Func<T, object> transform ) => new DecoratedSpecification<T>( @this, transform );
	}
}