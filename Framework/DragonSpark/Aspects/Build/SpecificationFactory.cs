using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	sealed class SpecificationFactory : IParameterizedSource<IEnumerable<Type>, Func<Type, bool>>
	{
		public static SpecificationFactory Default { get; } = new SpecificationFactory();
		SpecificationFactory() {}

		public Func<Type, bool> Get( IEnumerable<Type> parameter ) => 
			new Specification( parameter.Distinct().ToArray() ).ToSpecificationDelegate();
	}
}