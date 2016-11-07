using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Build
{
	public interface IDefinition : ISpecification<Type>, IParameterizedSource<Type, IEnumerable<AspectInstance>> {}
}