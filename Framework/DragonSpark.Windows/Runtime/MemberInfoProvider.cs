using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProvider : ComponentModel.TypeDefinitionProvider
	{
		public new static ICache<TypeInfo, TypeInfo> Instance { get; } = new TypeDefinitionProvider().Cached();

		TypeDefinitionProvider() : base( MetadataTypeDefinitionProvider.Instance ) {}
	}
}