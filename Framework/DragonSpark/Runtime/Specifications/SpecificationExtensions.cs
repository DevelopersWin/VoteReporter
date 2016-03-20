namespace DragonSpark.Runtime.Specifications
{
	public static class SpecificationExtensions
	{
		public static ISpecification<T> Inverse<T>( this ISpecification @this ) => @this.Inverse().Wrap<T>();

		public static ISpecification Inverse( this ISpecification @this ) => new InverseSpecification( @this );

		public static ISpecification Or( this ISpecification @this, ISpecification other ) => new AnySpecification( @this, other );

		public static ISpecification And( this ISpecification @this, ISpecification other ) => new AllSpecification( @this, other );

		public static ISpecification<T> Wrap<T>( this ISpecification @this ) => new DecoratedSpecification<T>( @this );
	}
}