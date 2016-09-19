using System;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Build
{
	sealed class SpecificationFactory : IParameterizedSource<IEnumerable<IDefinition>, Func<Type, bool>>
	{
		public static SpecificationFactory Default { get; } = new SpecificationFactory();
		SpecificationFactory() {}

		public Func<Type, bool> Get( IEnumerable<IDefinition> parameter ) => new Specification( parameter.Select( definition => definition.DeclaringType ).ToArray() ).ToSpecificationDelegate();
	}
}