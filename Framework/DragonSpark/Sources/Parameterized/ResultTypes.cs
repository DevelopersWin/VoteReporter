using System;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class ResultTypes : SourceTypeLocatorBase
	{
		public static IParameterizedSource<Type, Type> Default { get; } = new ResultTypes();
		ResultTypes() : base( types => types.Last(), typeof(IParameterizedSource<,>), typeof(ISource<>), typeof(Func<>), typeof(Func<,>) ) {}
	}
}