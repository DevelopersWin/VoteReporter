using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects.Definitions
{
	public interface ITypeDefinition : ISpecification<TypeInfo>, ITypeAware, IEnumerable<IMethods> {}
}