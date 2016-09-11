using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public interface IParameterValidationAdapter : ISpecification<MethodInfo>, ISpecification<object> {}
}