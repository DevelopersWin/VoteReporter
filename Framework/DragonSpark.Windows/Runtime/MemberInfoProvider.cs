using DragonSpark.Activation;
using System;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProvider : ComponentModel.TypeDefinitionProvider
	{
		public new static Func<TypeInfo, TypeInfo> Instance { get; } = new TypeDefinitionProvider().Cached();

		TypeDefinitionProvider() : base( MetadataTypeDefinitionProvider.Instance ) {}
	}
}