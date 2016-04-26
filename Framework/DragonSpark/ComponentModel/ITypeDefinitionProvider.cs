using DragonSpark.Activation;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public interface ITypeDefinitionProvider : ITransformer<TypeInfo>
	{
		/*var result = type.FromMetadata<MetadataTypeAttribute, Type>( item => item.MetadataClassType );
		return result;*/
		// TypeInfo GetDefinition( TypeInfo info );
	}
}