using DragonSpark.Runtime.Specifications;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class DerivedMethodSpecification : GuardedSpecificationBase<MethodInfo>
	{
		public static DerivedMethodSpecification Instance { get; } = new DerivedMethodSpecification();

		DerivedMethodSpecification() {}

		public override bool IsSatisfiedBy( MethodInfo parameter )
		{
			var methodInfo = parameter.GetRuntimeBaseDefinition();
			var result = methodInfo.DeclaringType != parameter.DeclaringType;
			return result;
		}
	}

	public class DerivedTypeSpecification : GuardedSpecificationBase<TypeInfo>
	{
		public static DerivedTypeSpecification Instance { get; } = new DerivedTypeSpecification();

		public override bool IsSatisfiedBy( TypeInfo parameter ) => false;// parameter.BaseType != typeof(object);
	}
}