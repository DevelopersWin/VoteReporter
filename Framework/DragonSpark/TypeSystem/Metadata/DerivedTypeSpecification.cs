using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.TypeSystem.Metadata
{
	public class DerivedTypeSpecification : SpecificationBase<TypeInfo>
	{
		public static DerivedTypeSpecification Default { get; } = new DerivedTypeSpecification();
		DerivedTypeSpecification() {}

		public override bool IsSatisfiedBy( TypeInfo parameter ) => /*false;//*/ parameter.BaseType != typeof(object);
	}
}