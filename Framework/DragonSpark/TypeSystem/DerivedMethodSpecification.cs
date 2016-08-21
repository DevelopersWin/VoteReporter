using DragonSpark.Runtime.Specifications;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class DerivedMethodSpecification : SpecificationBase<MethodInfo>
	{
		public static DerivedMethodSpecification Default { get; } = new DerivedMethodSpecification();

		DerivedMethodSpecification() {}

		public override bool IsSatisfiedBy( MethodInfo parameter )
		{
			var methodInfo = parameter.GetRuntimeBaseDefinition();
			var result = methodInfo.DeclaringType != parameter.DeclaringType;
			return result;
		}
	}

	public class DerivedTypeSpecification : SpecificationBase<TypeInfo>
	{
		public static DerivedTypeSpecification Default { get; } = new DerivedTypeSpecification();

		public override bool IsSatisfiedBy( TypeInfo parameter ) => false;// parameter.BaseType != typeof(object);
	}
}