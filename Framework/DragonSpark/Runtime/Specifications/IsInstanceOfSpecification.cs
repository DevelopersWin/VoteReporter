using DragonSpark.TypeSystem;

namespace DragonSpark.Runtime.Specifications
{
	public class IsInstanceOfSpecification<T> : SpecificationBase<object>
	{
		public static IsInstanceOfSpecification<T> Instance { get; } = new IsInstanceOfSpecification<T>();
		IsInstanceOfSpecification() : base( Where<object>.Always ) {}

		public override bool IsSatisfiedBy( object parameter ) => parameter is T;
	}
}