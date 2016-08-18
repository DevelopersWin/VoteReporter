namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProviderSource : DragonSpark.TypeSystem.TypeDefinitionProviderSource
	{
		public new static TypeDefinitionProviderSource Instance { get; } = new TypeDefinitionProviderSource();
		TypeDefinitionProviderSource() : base( MetadataTypeDefinitionProvider.Instance ) {}
	}
}