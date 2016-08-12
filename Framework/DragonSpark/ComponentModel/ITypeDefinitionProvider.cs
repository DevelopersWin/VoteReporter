using System.Reflection;
using DragonSpark.Sources;

namespace DragonSpark.ComponentModel
{
	public interface ITypeDefinitionProvider : IParameterizedSource<TypeInfo, TypeInfo>
	{
		/*var result = type.FromMetadata<MetadataTypeAttribute, Type>( item => item.MetadataClassType );
		return result;*/
		// TypeInfo GetDefinition( TypeInfo info );
	}
}