namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProvider : ComponentModel.TypeDefinitionProvider
	{
		public new static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider();

		TypeDefinitionProvider() : base( MetadataTypeDefinitionProvider.Instance ) {}
	}
}