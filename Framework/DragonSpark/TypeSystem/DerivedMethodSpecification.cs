using DragonSpark.Runtime.Specifications;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class DerivedMethodSpecification : CoercedSpecificationBase<MethodInfo>
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
}