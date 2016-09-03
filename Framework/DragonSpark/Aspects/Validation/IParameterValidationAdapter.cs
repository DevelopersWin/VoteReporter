using DragonSpark.Specifications;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public interface IParameterValidationAdapter : ISpecification<MethodInfo>, ISpecification<object> {}
}