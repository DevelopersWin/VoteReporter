using System;
using DragonSpark.Coercion;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Coercion
{
	class Constructor : AdapterConstructorSource<ICoercer>
	{
		public static IParameterizedSource<Type, Func<object, ICoercer>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( typeof(ICoercer<,>), typeof(CoercerAdapter<,>) ) {}
	}
}