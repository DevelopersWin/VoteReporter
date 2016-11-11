using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspectBuildDefinition : ISpecification<TypeInfo>, IAspectProvider<TypeInfo>, IEnumerable<Type> {}
}