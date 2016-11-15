using System;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem
{
	public sealed class InnerTypes : Cache<Type, Type>
	{
		public static InnerTypes Default { get; } = new InnerTypes();
		InnerTypes() : base( TypeLocator.Default.Get ) {}
	}
}