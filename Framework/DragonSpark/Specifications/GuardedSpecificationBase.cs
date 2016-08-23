namespace DragonSpark.Specifications
{
	public class InverseSpecification<T> : DecoratedSpecification<T>
	{
		public InverseSpecification( ISpecification<T> inner ) : base( inner ) {}

		public override bool IsSatisfiedBy( T parameter ) => !base.IsSatisfiedBy( parameter );
	}

	/*public abstract class GuardedSpecificationBase<T> : SpecificationBase<T>
	{
		protected GuardedSpecificationBase() : this( Defaults<T>.Coercer ) {}
		protected GuardedSpecificationBase( Coerce<T> coercer ) : base( coercer, Where<T>.Assigned ) {}
	}*/
}