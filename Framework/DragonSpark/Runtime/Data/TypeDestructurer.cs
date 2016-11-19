using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Runtime.Data
{
	public sealed class TypeDestructurer : DelegatedParameterizedSource<Type, string>
	{
		public static TypeDestructurer Default { get; } = new TypeDestructurer();
		TypeDestructurer() : base( type => type.AssemblyQualifiedName ) {}
	}
}