using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Runtime.Data
{
	public sealed class TypeSelector : DelegatedParameterizedSource<string, Type>
	{
		public static TypeSelector Default { get; } = new TypeSelector();
		TypeSelector() : base( Type.GetType ) {}
	}
}