using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Linq;

namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		// public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );


		public static ISpecification<T> Inverse<T>( this ISpecification<T> @this ) => new InverseSpecification( @this ).Cast<T>();
		

		// public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification<T> Or<T>( this ISpecification<T> @this, params ISpecification[] others ) 
			=> new AnySpecification<T>( @this.Append( others.Select( specification =>  specification.Cast<T>() ) ).Fixed() );

		// public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );

		public static ISpecification<T> And<T>( this ISpecification<T> @this, params ISpecification[] others ) 
			=> new AllSpecification<T>( @this.Append( others.Select( specification =>  specification.Cast<T>() ) ).Fixed() );

		public static ISpecification<T> Cast<T>( this ISpecification @this ) => @this.Cast( Delegates<T>.Object );

		public static ISpecification<T> Cast<T>( this ISpecification @this, Func<T, object> projection ) => @this as ISpecification<T> ?? new DecoratedSpecification<T>( @this, projection );

		// public static ISpecification<T> Cast<T>( this ISpecification<T> @this ) => @this.Cast( Default<T>.Boxed );
	}
}