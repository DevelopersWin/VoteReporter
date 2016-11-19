using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime.Data
{
	public sealed class TypeParser : ParameterizedItemSourceBase<string, Type>
	{
		public static TypeParser Default { get; } = new TypeParser();
		TypeParser() {}

		public override IEnumerable<Type> Yield( string parameter ) => parameter.ToStringArray().SelectAssigned( TypeSelector.Default.Get );
	}
}
