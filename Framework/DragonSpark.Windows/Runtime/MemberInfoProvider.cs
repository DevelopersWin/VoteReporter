namespace DragonSpark.Windows.Runtime
{
	public class TypeDefinitionProviderStore : DragonSpark.TypeSystem.TypeDefinitionProviderStore
	{
		public new static TypeDefinitionProviderStore Instance { get; } = new TypeDefinitionProviderStore();
		TypeDefinitionProviderStore() : base( MetadataTypeDefinitionProvider.Instance ) {}
	}
}