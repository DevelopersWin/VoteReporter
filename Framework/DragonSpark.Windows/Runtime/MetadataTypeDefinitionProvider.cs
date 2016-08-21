using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Runtime
{
	public class MetadataTypeDefinitionProvider : TransformerBase<TypeInfo>, ITypeDefinitionProvider
	{
		public static MetadataTypeDefinitionProvider Default { get; } = new MetadataTypeDefinitionProvider();
		MetadataTypeDefinitionProvider() {}

		public override TypeInfo Get( TypeInfo parameter ) => parameter.GetCustomAttribute<MetadataTypeAttribute>().With( item => item.MetadataClassType.GetTypeInfo() );
	}
}
