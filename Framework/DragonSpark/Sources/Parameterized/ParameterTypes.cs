using DragonSpark.Commands;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class ParameterTypes : TypeLocatorBase
	{
		public static ICache<Type, Type> Default { get; } = new ParameterTypes();
		ParameterTypes() : base( typeof(Func<,>), typeof(IParameterizedSource<,>), typeof(ICommand<>), typeof(ISpecification<>) ) {}

		protected override Type From( IEnumerable<Type> genericTypeArguments ) => genericTypeArguments.First();
	}
}