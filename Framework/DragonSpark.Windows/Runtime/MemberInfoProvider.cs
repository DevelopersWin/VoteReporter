namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProviderSource : DragonSpark.TypeSystem.Metadata.TypeDefinitionProviderSource
	{
		public new static TypeDefinitionProviderSource Default { get; } = new TypeDefinitionProviderSource();
		TypeDefinitionProviderSource() : base( MetadataTypeDefinitionProvider.Default ) {}
	}
}