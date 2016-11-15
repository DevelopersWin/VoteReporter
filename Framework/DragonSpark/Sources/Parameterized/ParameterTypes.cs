using DragonSpark.Commands;
using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class ParameterTypes : SourceTypeLocatorBase
	{
		public static IParameterizedSource<Type, Type> Default { get; } = new ParameterTypes();
		ParameterTypes() : base( types => types.First(), typeof(Func<,>), typeof(IParameterizedSource<,>), typeof(ICommand<>), typeof(ISpecification<>) ) {}
	}
}