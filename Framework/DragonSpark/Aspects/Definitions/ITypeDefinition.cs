using DragonSpark.Aspects.Build;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Definitions
{
	public interface ITypeDefinition : ISpecification<Type>, ITypeAware, IEnumerable<IMethods> {}
}