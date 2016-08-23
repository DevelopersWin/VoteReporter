using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.TypeSystem
{
	public class DerivedMethodSpecification : SpecificationBase<MethodInfo>
	{
		public static DerivedMethodSpecification Default { get; } = new DerivedMethodSpecification();
		DerivedMethodSpecification() {}

		public override bool IsSatisfiedBy( MethodInfo parameter ) => 
			parameter.GetRuntimeBaseDefinition().DeclaringType != parameter.DeclaringType;
	}

	public class DerivedTypeSpecification : SpecificationBase<TypeInfo>
	{
		public static DerivedTypeSpecification Default { get; } = new DerivedTypeSpecification();
		DerivedTypeSpecification() {}

		public override bool IsSatisfiedBy( TypeInfo parameter ) => /*false;//*/ parameter.BaseType != typeof(object);
	}
}