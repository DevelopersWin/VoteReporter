using DragonSpark.Runtime.Specifications;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class DerivedMethodSpecification : SpecificationBase<MethodInfo>
	{
		public static DerivedMethodSpecification Instance { get; } = new DerivedMethodSpecification();

		DerivedMethodSpecification() {}

		protected override bool Verify( MethodInfo parameter )
		{
			var methodInfo = parameter.GetRuntimeBaseDefinition();
			var result = methodInfo.DeclaringType != parameter.DeclaringType;
			return result;
		}
	}
}